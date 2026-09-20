using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Models;

public sealed class UserModel : ModelBase
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
}
