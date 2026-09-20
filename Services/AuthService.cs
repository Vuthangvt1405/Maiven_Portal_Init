using Maiven_Portal_Managment.Dtos;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Repository.Interfaces;
using Maiven_Portal_Managment.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Maiven_Portal_Managment.Services;

public sealed class AuthService(
    IAuthRepository authRepository,
    IPasswordHasher<UserModel> passwordHasher,
    ITokenService tokenService) : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    public async Task<AuthResponse> RegisterStudentAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
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

        return CreateAuthResponse(account);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
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

        if (verificationResult == PasswordVerificationResult.Failed || account.RoleCodes.Count == 0)
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

        return CreateAuthResponse(account);
    }

    private AuthResponse CreateAuthResponse(AuthAccount account)
    {
        var roles = account.RoleCodes
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var accessToken = tokenService.CreateAccessToken(account.User, roles);

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
                Roles = roles
            }
        };
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
