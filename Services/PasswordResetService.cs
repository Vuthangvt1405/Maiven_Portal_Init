using System.Security.Cryptography;
using System.Text;
using log4net;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Repository;
using Microsoft.AspNetCore.Identity;

namespace Maiven_Portal_Managment.Services;

public sealed class PasswordResetService(
    PasswordResetRepository passwordResetRepository,
    IPasswordHasher<User> passwordHasher,
    IPasswordHasher<PasswordResetRequest> otpHasher,
    IEmailSender emailSender)
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(PasswordResetService));
    private const int OtpLifetimeMinutes = 10;
    private const int ResetTokenLifetimeMinutes = 10;
    private const int MaximumOtpAttempts = 5;
    private const string InvalidOtpMessage = "The password reset code is invalid or has expired.";
    private const string InvalidTokenMessage = "The password reset token is invalid or has expired.";

    public async Task RequestOtpAsync(RequestPasswordResetOtpRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await passwordResetRepository.FindUserByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return;
        }

        var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var resetRequest = new PasswordResetRequest
        {
            UserId = user.Id,
            Method = "OTP",
            OtpHash = otpHasher.HashPassword(new PasswordResetRequest(), otp),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(OtpLifetimeMinutes)
        };
        await passwordResetRepository.CreateAsync(resetRequest, cancellationToken);
        await emailSender.SendPasswordResetOtpAsync(user.Email, otp, cancellationToken);
        Logger.Info($"Password reset OTP requested UserId={user.Id}");
    }

    public async Task<PasswordResetTokenResponse> VerifyOtpAsync(VerifyPasswordResetOtpRequest request, CancellationToken cancellationToken)
    {
        var user = await passwordResetRepository.FindUserByEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        if (user is null)
        {
            throw new BadRequestException(InvalidOtpMessage);
        }

        var resetRequest = await passwordResetRepository.GetLatestOtpAsync(user.Id, cancellationToken);
        if (resetRequest is null || resetRequest.ExpiresAtUtc <= DateTime.UtcNow || resetRequest.OtpAttemptCount >= MaximumOtpAttempts || string.IsNullOrWhiteSpace(resetRequest.OtpHash))
        {
            throw new BadRequestException(InvalidOtpMessage);
        }

        var result = otpHasher.VerifyHashedPassword(resetRequest, resetRequest.OtpHash, request.Otp);
        if (result == PasswordVerificationResult.Failed)
        {
            resetRequest.OtpAttemptCount++;
            if (resetRequest.OtpAttemptCount >= MaximumOtpAttempts)
            {
                resetRequest.InvalidatedAtUtc = DateTime.UtcNow;
            }
            await passwordResetRepository.SaveChangesAsync(cancellationToken);
            throw new BadRequestException(InvalidOtpMessage);
        }

        return await IssueTokenAsync(resetRequest, cancellationToken);
    }

    public async Task<PasswordResetTokenResponse> CreateFaceResetTokenAsync(long userId, CancellationToken cancellationToken)
    {
        var resetRequest = new PasswordResetRequest
        {
            UserId = userId,
            Method = "FACE",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(ResetTokenLifetimeMinutes)
        };
        await passwordResetRepository.CreateAsync(resetRequest, cancellationToken);
        var response = await IssueTokenAsync(resetRequest, cancellationToken);
        Logger.Info($"Face password reset verified UserId={userId}");
        return response;
    }

    public async Task ConfirmAsync(ConfirmPasswordResetRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.ResetToken);
        var resetRequest = await passwordResetRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (resetRequest is null || resetRequest.ConsumedAtUtc is not null || resetRequest.InvalidatedAtUtc is not null || resetRequest.VerifiedAtUtc is null || resetRequest.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new BadRequestException(InvalidTokenMessage);
        }

        resetRequest.User.PasswordHash = passwordHasher.HashPassword(resetRequest.User, request.NewPassword);
        resetRequest.ConsumedAtUtc = DateTime.UtcNow;
        await passwordResetRepository.SaveChangesAsync(cancellationToken);
        Logger.Info($"Password reset completed UserId={resetRequest.UserId} Method={resetRequest.Method}");
    }

    private async Task<PasswordResetTokenResponse> IssueTokenAsync(PasswordResetRequest resetRequest, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        resetRequest.ResetTokenHash = HashToken(token);
        resetRequest.VerifiedAtUtc = now;
        resetRequest.ExpiresAtUtc = now.AddMinutes(ResetTokenLifetimeMinutes);
        await passwordResetRepository.SaveChangesAsync(cancellationToken);
        return new PasswordResetTokenResponse { ResetToken = token, ExpiresAtUtc = resetRequest.ExpiresAtUtc };
    }

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
