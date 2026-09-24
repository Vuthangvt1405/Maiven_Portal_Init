using log4net;
using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Configuration;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Repository;
using Maiven_Portal_Managment.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Maiven_Portal_Managment.Services;

public sealed class FaceCredentialService(
    FaceCredentialRepository faceCredentialRepository,
    UserRepository userRepository,
    FaceRecognitionService faceRecognitionService,
    FaceRegisterSessionService faceRegisterSessionService,
    CurrentUserContext currentUserContext,
    IPasswordHasher<User> passwordHasher,
    IOptions<FaceRecognitionOptions> optionsAccessor)
{
    private static readonly ILog Logger =
        LogManager.GetLogger(typeof(FaceCredentialService));

    private readonly FaceRecognitionOptions options = optionsAccessor.Value;

    public async Task<FaceStatusResponse> GetFaceStatusAsync(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentStudentUserId();
        var credential = await faceCredentialRepository.GetActiveByUserIdAsync(
            userId,
            cancellationToken);

        return new FaceStatusResponse
        {
            IsRegistered = credential is not null,
            RegisteredAt = credential?.CreatedAt,
            ModelName = credential?.ModelName
        };
    }

    public async Task<FaceRegisterResponse> StartFaceRegistrationAsync(
        FaceRegisterRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = GetCurrentStudentUserId();
        await VerifyCurrentStudentPasswordAsync(
            userId,
            request.Password,
            cancellationToken);

        var response = faceRegisterSessionService.StartSession(userId);
        Logger.Info(
            $"Face registration started UserId={userId} SessionId={response.SessionId}");
        return response;
    }

    public async Task<FaceFrameResponse> UploadFaceFrameAsync(
        Guid sessionId,
        IFormFile frame,
        CancellationToken cancellationToken)
    {
        if (frame is null)
        {
            throw new BadRequestException("A face image is required.");
        }

        var userId = GetCurrentStudentUserId();
        var currentStatus = faceRegisterSessionService.GetFrameStatus(
            sessionId,
            userId);

        if (currentStatus.IsComplete)
        {
            return currentStatus;
        }

        var embedding = await faceRecognitionService.TryCreateEmbeddingAsync(
            frame,
            cancellationToken);

        if (embedding is null)
        {
            return currentStatus;
        }

        var response = faceRegisterSessionService.AddEmbedding(
            sessionId,
            userId,
            embedding);
        Logger.Info(
            $"Face frame accepted UserId={userId} SessionId={sessionId} " +
            $"AcceptedCount={response.AcceptedCount} RequiredCount={response.RequiredCount}");
        return response;
    }

    public async Task<FaceVerifyResponse> ConfirmFaceRegistrationAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentStudentUserId();
        var embeddings = faceRegisterSessionService.GetCompletedEmbeddings(
            sessionId,
            userId);

        if (!await IsActiveStudentAsync(userId, cancellationToken))
        {
            faceRegisterSessionService.CancelSession(sessionId, userId);
            throw new UnauthorizedException(
                "An active Student account is required.");
        }

        var representativeEmbedding =
            faceRecognitionService.CreateRepresentativeEmbedding(embeddings);

        var otherCredentials =
            await faceCredentialRepository.GetActiveForModelExceptUserAsync(
                faceRecognitionService.ModelName,
                faceRecognitionService.ModelVersion,
                representativeEmbedding.Length,
                userId,
                cancellationToken);

        foreach (var otherCredential in otherCredentials)
        {
            var otherEmbedding = DeserializeEmbedding(otherCredential);
            var similarity = FaceRecognitionService.CosineSimilarity(
                representativeEmbedding,
                otherEmbedding);

            if (similarity >= options.DuplicateThreshold)
            {
                throw new ConflictException(
                    "This face is already registered to another account.");
            }
        }

        var credential = new FaceCredential
        {
            UserId = userId,
            Embedding = SerializeEmbedding(representativeEmbedding),
            EmbeddingDimension = representativeEmbedding.Length,
            ModelName = faceRecognitionService.ModelName,
            ModelVersion = faceRecognitionService.ModelVersion
        };

        await faceCredentialRepository.ReplaceAsync(credential, cancellationToken);
        faceRegisterSessionService.CompleteSession(sessionId, userId);

        Logger.Info(
            $"Face credential registered UserId={userId} " +
            $"ModelName={credential.ModelName} ModelVersion={credential.ModelVersion}");

        return new FaceVerifyResponse
        {
            Registered = true,
            RegisteredAt = credential.CreatedAt,
            ModelName = credential.ModelName,
            ModelVersion = credential.ModelVersion
        };
    }

    public Task CancelFaceRegistrationAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var userId = GetCurrentStudentUserId();
        faceRegisterSessionService.CancelSession(sessionId, userId);
        Logger.Info(
            $"Face registration cancelled UserId={userId} SessionId={sessionId}");
        return Task.CompletedTask;
    }

    public async Task DeleteFaceAsync(
        DeleteFaceRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = GetCurrentStudentUserId();
        await VerifyCurrentStudentPasswordAsync(
            userId,
            request.Password,
            cancellationToken);

        var deleted = await faceCredentialRepository.SoftDeleteByUserIdAsync(
            userId,
            cancellationToken);

        Logger.Info($"Face credential delete requested UserId={userId} Deleted={deleted}");
    }

    private long GetCurrentStudentUserId()
    {
        if (!currentUserContext.IsAuthenticated ||
            currentUserContext.UserId is not long userId ||
            !string.Equals(
                currentUserContext.Role,
                SystemRoles.Student.Code,
                StringComparison.Ordinal))
        {
            throw new UnauthorizedException(
                "An authenticated Student account is required.");
        }

        return userId;
    }

    private async Task VerifyCurrentStudentPasswordAsync(
        long userId,
        string password,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new UnauthorizedException("The password is incorrect.");
        }

        var user = await userRepository.GetTrackedByIdAsync(
            userId,
            cancellationToken);

        if (user is null ||
            !user.UserRoles.Any(userRole =>
                string.Equals(
                    userRole.Role.Code,
                    SystemRoles.Student.Code,
                    StringComparison.Ordinal)))
        {
            throw new UnauthorizedException(
                "An active Student account is required.");
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException("The password is incorrect.");
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            await userRepository.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<bool> IsActiveStudentAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        return user is not null &&
            user.UserRoles.Any(userRole =>
                string.Equals(
                    userRole.Role.Code,
                    SystemRoles.Student.Code,
                    StringComparison.Ordinal));
    }

    private static byte[] SerializeEmbedding(float[] embedding)
    {
        var bytes = new byte[embedding.Length * sizeof(float)];
        Buffer.BlockCopy(embedding, 0, bytes, 0, bytes.Length);
        return bytes;
    }

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
