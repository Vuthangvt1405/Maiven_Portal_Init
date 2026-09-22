using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class UserProfileResponse
{
    public long Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
    public long RoleUserId { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    public Gender? Gender { get; init; }

    public string? Phone { get; init; }

    public string? Address { get; init; }

    public string? AvatarUrl { get; init; }

}