namespace Maiven_Portal_Managment.Services.Security;

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);
