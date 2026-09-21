namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class CurrentUserResponse
{
    public long Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public long RoleUserId { get; init; }
}
