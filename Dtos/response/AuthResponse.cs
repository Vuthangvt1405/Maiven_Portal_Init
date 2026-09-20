namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public DateTime ExpiresAtUtc { get; init; }
    public AuthUserResponse User { get; init; } = new();
}
