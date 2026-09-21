namespace Maiven_Portal_Managment.Dtos;

public sealed class AuthRoleAssignment
{
    public long RoleUserId { get; init; }
    public string RoleCode { get; init; } = string.Empty;
}
