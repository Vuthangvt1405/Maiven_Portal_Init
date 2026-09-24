using log4net;
using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Configuration;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Logging;
using Maiven_Portal_Managment.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Maiven_Portal_Managment.Services;

public sealed class AuthService(
    AuthRepository authRepository,
    FaceCredentialRepository faceCredentialRepository,
    FaceRecognitionService faceRecognitionService,
    IPasswordHasher<User> passwordHasher,
    JwtTokenService tokenService,
    IOptions<FaceRecognitionOptions> faceOptionsAccessor)
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthService));
    private const string InvalidCredentialsMessage = "Invalid email or password.";
    private const string FaceVerificationFailedMessage = "Face verification failed.";
    private const int RequiredFaceLoginFrames = 3;
    private readonly FaceRecognitionOptions faceOptions = faceOptionsAccessor.Value;

    public async Task<AuthResponse> RegisterStudentAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        if (await authRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new User
        {
            Email = normalizedEmail,
            FullName = request.FullName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Phone = NormalizeOptional(request.Phone),
            Address = NormalizeOptional(request.Address),
            AvatarUrl = NormalizeOptional(request.AvatarUrl)
        };
        var passwordHash = passwordHasher.HashPassword(user, request.Password);
        var account = await CreateAccountWithRoleAsync(
            user,
            passwordHash,
            SystemRoles.Student.Code,
            cancellationToken);

        var response = CreateAuthResponse(account, account.RoleAssignments.Single());
        Logger.Info($"Student registered UserId={response.User.Id} Email={LogFormat.FormatValue(response.User.Email)} Role={LogFormat.FormatValue(response.User.Role)}");
        return response;
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await AuthenticateAsync(request, isAdminLogin: false, cancellationToken);
        return response;
    }

    public async Task<AuthResponse> LoginAdminAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await AuthenticateAsync(request, isAdminLogin: true, cancellationToken);
        return response;
    }

    public async Task<AuthResponse> FaceLoginAsync(
        IReadOnlyList<IFormFile> frames,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(frames);

        if (frames.Count != RequiredFaceLoginFrames)
        {
            throw new BadRequestException(
                $"Exactly {RequiredFaceLoginFrames} face images are required.");
        }

        var embeddings = new List<float[]>(RequiredFaceLoginFrames);
        for (var index = 0; index < frames.Count; index++)
        {
            var embedding = await faceRecognitionService.TryCreateEmbeddingAsync(
                frames[index],
                cancellationToken);

            if (embedding is null)
            {
                Logger.Warn(
                    $"Face login rejected Reason=FrameValidationFailed " +
                    $"FrameNumber={index + 1}");
                throw new UnauthorizedException(FaceVerificationFailedMessage);
            }

            embeddings.Add(embedding);
        }

        var representativeEmbedding =
            faceRecognitionService.CreateRepresentativeEmbedding(embeddings);
        var credentials = await faceCredentialRepository.GetActiveForModelAsync(
            faceRecognitionService.ModelName,
            faceRecognitionService.ModelVersion,
            representativeEmbedding.Length,
            cancellationToken);

        var rankedMatches = credentials
            .Select(credential => new
            {
                Credential = credential,
                Similarity = FaceRecognitionService.CosineSimilarity(
                    representativeEmbedding,
                    DeserializeEmbedding(credential))
            })
            .OrderByDescending(match => match.Similarity)
            .ToArray();

        if (rankedMatches.Length == 0)
        {
            Logger.Warn(
                $"Face login rejected Reason=NoCompatibleCredential " +
                $"ModelName={faceRecognitionService.ModelName} " +
                $"ModelVersion={faceRecognitionService.ModelVersion} " +
                $"EmbeddingDimension={representativeEmbedding.Length}");
            throw new UnauthorizedException(FaceVerificationFailedMessage);
        }

        var top1 = rankedMatches[0];
        var top2Similarity = rankedMatches.Length > 1
            ? rankedMatches[1].Similarity
            : (double?)null;
        var margin = top2Similarity.HasValue
            ? top1.Similarity - top2Similarity.Value
            : (double?)null;

        Logger.Info(
            $"Face login comparison CandidateCount={rankedMatches.Length} " +
            $"Top1CredentialId={top1.Credential.Id} " +
            $"Top1UserId={top1.Credential.UserId} " +
            $"Top1Similarity={top1.Similarity:F4} " +
            $"Top2Similarity={(top2Similarity.HasValue ? top2Similarity.Value.ToString("F4") : "N/A")} " +
            $"Margin={(margin.HasValue ? margin.Value.ToString("F4") : "N/A")} " +
            $"MatchThreshold={faceOptions.MatchThreshold:F4} " +
            $"MinMargin={faceOptions.MinMargin:F4}");

        if (top1.Similarity < faceOptions.MatchThreshold)
        {
            Logger.Warn(
                $"Face login rejected Reason=BelowMatchThreshold " +
                $"Top1Similarity={top1.Similarity:F4} " +
                $"MatchThreshold={faceOptions.MatchThreshold:F4}");
            throw new UnauthorizedException(FaceVerificationFailedMessage);
        }

        if (margin.HasValue && margin.Value < faceOptions.MinMargin)
        {
            Logger.Warn(
                $"Face login rejected Reason=InsufficientMargin " +
                $"Margin={margin.Value:F4} MinMargin={faceOptions.MinMargin:F4}");
            throw new UnauthorizedException(FaceVerificationFailedMessage);
        }

        var account = await authRepository.FindByUserIdAsync(
            top1.Credential.UserId,
            cancellationToken);
        var roleAssignment = account?.RoleAssignments.Count == 1
            ? account.RoleAssignments.Single()
            : null;

        if (account is null ||
            roleAssignment is null ||
            !string.Equals(
                roleAssignment.RoleCode,
                SystemRoles.Student.Code,
                StringComparison.Ordinal))
        {
            Logger.Warn(
                $"Face login rejected Reason=MatchedStudentUnavailable " +
                $"CandidateUserId={top1.Credential.UserId}");
            throw new UnauthorizedException(FaceVerificationFailedMessage);
        }

        var response = CreateAuthResponse(account, roleAssignment);
        Logger.Info(
            $"Face login succeeded UserId={response.User.Id} " +
            $"Similarity={top1.Similarity:F4}");
        return response;
    }

    private async Task<AuthResponse> AuthenticateAsync(
        LoginRequest request,
        bool isAdminLogin,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var account = await authRepository.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (account is null)
        {
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(
            account.User,
            account.PasswordHash,
            request.Password);
        var roleAssignment = account.RoleAssignments.Count == 1
            ? account.RoleAssignments.Single()
            : null;
        var hasAllowedRole = IsAllowedLoginRole(
            roleAssignment?.RoleCode,
            isAdminLogin);

        if (verificationResult == PasswordVerificationResult.Failed ||
            roleAssignment is null ||
            !hasAllowedRole)
        {
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var upgradedHash = passwordHasher.HashPassword(account.User, request.Password);
            await authRepository.UpdatePasswordHashAsync(
                account.User.Id,
                upgradedHash,
                cancellationToken);
            Logger.Info($"Password hash upgraded UserId={account.User.Id}");
        }

        return CreateAuthResponse(account, roleAssignment);
    }

    private async Task<AuthAccount> CreateAccountWithRoleAsync(
        User user,
        string passwordHash,
        string roleCode,
        CancellationToken cancellationToken)
    {
        var role = await authRepository.GetRoleByCodeAsync(roleCode, cancellationToken)
            ?? throw new InvalidOperationException(
                $"The required {roleCode} role is not configured.");

        user.PasswordHash = passwordHash;
        user.UserRoles.Add(new UserRole
        {
            RoleId = role.Id
        });

        try
        {
            await authRepository.AddUserAsync(user, cancellationToken);
        }
        catch (DbUpdateException exception)
            when (AuthRepository.IsUniqueConstraintViolation(exception))
        {
            throw new ConflictException("An account with this email already exists.", exception);
        }

        var userRole = user.UserRoles.Single();

        return new AuthAccount
        {
            User = user,
            PasswordHash = user.PasswordHash,
            RoleAssignments =
            [
                new AuthRoleAssignment
                {
                    RoleUserId = userRole.Id,
                    RoleCode = role.Code
                }
            ]
        };
    }

    private AuthResponse CreateAuthResponse(
        AuthAccount account,
        AuthRoleAssignment roleAssignment)
    {
        var accessToken = tokenService.CreateAccessToken(
            account.User,
            roleAssignment);

        return new AuthResponse
        {
            AccessToken = accessToken.Token,
            ExpiresAtUtc = accessToken.ExpiresAtUtc,
            User = new AuthUserResponse
            {
                Id = account.User.Id,
                Email = account.User.Email,
                FullName = account.User.FullName,
                DateOfBirth = account.User.DateOfBirth,
                Gender = account.User.Gender,
                Phone = account.User.Phone,
                Address = account.User.Address,
                AvatarUrl = account.User.AvatarUrl,
                Role = roleAssignment.RoleCode,
                RoleUserId = roleAssignment.RoleUserId
            }
        };
    }

    private static bool IsAllowedLoginRole(
        string? roleCode,
        bool isAdminLogin) =>
        isAdminLogin
            ? string.Equals(roleCode, SystemRoles.Admin.Code, StringComparison.Ordinal)
            : string.Equals(roleCode, SystemRoles.Student.Code, StringComparison.Ordinal) ||
              string.Equals(roleCode, SystemRoles.Teacher.Code, StringComparison.Ordinal);

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static float[] DeserializeEmbedding(FaceCredential credential)
    {
        if (credential.Embedding is null ||
            credential.EmbeddingDimension <= 0 ||
            credential.Embedding.Length != credential.EmbeddingDimension * sizeof(float))
        {
            throw new InvalidOperationException(
                $"Face credential {credential.Id} contains an invalid embedding.");
        }

        var embedding = new float[credential.EmbeddingDimension];
        Buffer.BlockCopy(
            credential.Embedding,
            0,
            embedding,
            0,
            credential.Embedding.Length);
        return embedding;
    }
}
