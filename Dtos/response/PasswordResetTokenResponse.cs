namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class PasswordResetTokenResponse
{
    public string ResetToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
