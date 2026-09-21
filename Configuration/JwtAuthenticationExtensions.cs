using System.Text;
using Maiven_Portal_Managment.Services;
using Maiven_Portal_Managment.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Maiven_Portal_Managment.Configuration;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtIssuer = jwtSection[nameof(JwtOptions.Issuer)] ?? string.Empty;
        var jwtAudience = jwtSection[nameof(JwtOptions.Audience)] ?? string.Empty;
        var jwtSecret = jwtSection[nameof(JwtOptions.Secret)] ?? string.Empty;

        services
            .AddOptions<JwtOptions>()
            .Bind(jwtSection)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
            .Validate(
                options => Encoding.UTF8.GetByteCount(options.Secret ?? string.Empty) >= JwtOptions.MinimumSecretLength,
                $"JWT secret must be at least {JwtOptions.MinimumSecretLength} bytes.")
            .Validate(options => options.AccessTokenMinutes > 0, "JWT access token lifetime must be positive.")
            .ValidateOnStart();

        //register this can help another service can see the current user context, eg: CurrentUserContext can ask through HttpContext to get the current user info
        services.AddHttpContextAccessor();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<CurrentUserContext>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.IncludeErrorDetails = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "email",
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorization();
        return services;
    }
}
