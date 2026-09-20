using System.IdentityModel.Tokens.Jwt;

namespace Maiven_Portal_Managment.Services.Security;

public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor)
{
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;

    public bool IsAuthenticated =>
        HttpContext?.User.Identity?.IsAuthenticated == true;

    public long? UserId
    {
        get
        {
            var subject = HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return long.TryParse(subject, out var userId) ? userId : null;
        }
    }

    public string? Email =>
        HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

    public IReadOnlyCollection<string> Roles => HttpContext?.User
        .FindAll("role")
        .Select(claim => claim.Value)
        .Where(role => !string.IsNullOrWhiteSpace(role))
        .Distinct(StringComparer.Ordinal)
        .ToArray() ?? [];
}
