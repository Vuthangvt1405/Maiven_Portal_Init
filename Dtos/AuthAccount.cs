using Maiven_Portal_Managment.Models;

namespace Maiven_Portal_Managment.Dtos;

public sealed class AuthAccount
{
    public UserModel User { get; init; } = new();
    public string PasswordHash { get; init; } = string.Empty;
    public IReadOnlyCollection<string> RoleCodes { get; init; } = [];
}
