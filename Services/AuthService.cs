using Maiven_Portal_Managment.Dtos;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Logging;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Repository;
using Microsoft.AspNetCore.Identity;

namespace Maiven_Portal_Managment.Services;

public sealed class AuthService(
    AuthRepository authRepository,
    IPasswordHasher<UserModel> passwordHasher,
    JwtTokenService tokenService,
    ActionLogService actionLogService)
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    public async Task<AuthResponse> RegisterStudentAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<AuthService>(
            "Service",
            nameof(RegisterStudentAsync));
        var normalizedEmail = NormalizeEmail(request.Email);

        if (await authRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new UserModel
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
        var account = await authRepository.CreateStudentAsync(
            user,
            passwordHash,
            cancellationToken);

        var response = CreateAuthResponse(account, account.RoleAssignments.Single());
        operation.Complete(
            ("UserId", response.User.Id),
            ("Role", response.User.Role));
        return response;
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<AuthService>(
            "Service",
            nameof(LoginAsync),
            ("LoginType", "User"));
        var response = await AuthenticateAsync(request, isAdminLogin: false, cancellationToken);
        operation.Complete(
            ("UserId", response.User.Id),
            ("Role", response.User.Role));
        return response;
    }

    public async Task<AuthResponse> LoginAdminAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<AuthService>(
            "Service",
            nameof(LoginAdminAsync),
            ("LoginType", "Admin"));
        var response = await AuthenticateAsync(request, isAdminLogin: true, cancellationToken);
        operation.Complete(
            ("UserId", response.User.Id),
            ("Role", response.User.Role));
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
        }

        return CreateAuthResponse(account, roleAssignment);
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
}
