namespace Maiven_Portal_Managment.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinimumSecretLength = 32;

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
}
