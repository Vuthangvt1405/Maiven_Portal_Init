using Maiven_Portal_Managment.Data.Entities;

namespace Maiven_Portal_Managment.Dtos;

public sealed class AuthAccount
{
    public User User { get; init; } = new();
    public string PasswordHash { get; init; } = string.Empty;
    public IReadOnlyCollection<AuthRoleAssignment> RoleAssignments { get; init; } = [];
}
