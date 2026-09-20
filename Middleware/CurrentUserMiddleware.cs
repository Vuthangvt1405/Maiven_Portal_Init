using System.IdentityModel.Tokens.Jwt;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Middleware;

public sealed class CurrentUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        CurrentUserContext currentUserContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var subject = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var email = context.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (!long.TryParse(subject, out var userId) || string.IsNullOrWhiteSpace(email))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var roles = context.User
                .FindAll("role")
                .Select(claim => claim.Value)
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            currentUserContext.SetAuthenticatedUser(userId, email, roles);
        }

        await next(context);
    }
}
