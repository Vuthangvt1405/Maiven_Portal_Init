namespace Maiven_Portal_Managment.Services.Interfaces;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    long? UserId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
}
