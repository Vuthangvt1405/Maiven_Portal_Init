namespace Maiven_Portal_Managment.Data.Entities;

public sealed class PasswordResetRequest : EntityBase
{
    public long UserId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? OtpHash { get; set; }
    public string? ResetTokenHash { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public DateTime? ConsumedAtUtc { get; set; }
    public DateTime? InvalidatedAtUtc { get; set; }
    public int OtpAttemptCount { get; set; }

    public User User { get; set; } = null!;
}
