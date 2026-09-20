using Maiven_Portal_Managment.Services.Interfaces;

namespace Maiven_Portal_Managment.Services.Security;

public sealed class CurrentUserContext : ICurrentUserContext
{
    public bool IsAuthenticated { get; private set; }
    public long? UserId { get; private set; }
    public string? Email { get; private set; }
    public IReadOnlyCollection<string> Roles { get; private set; } = [];

    internal void SetAuthenticatedUser(
        long userId,
        string email,
        IReadOnlyCollection<string> roles)
    {
        IsAuthenticated = true;
        UserId = userId;
        Email = email;
        Roles = roles;
    }
}
