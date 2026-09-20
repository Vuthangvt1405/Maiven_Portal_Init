using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Data.Entities;

public class User : EntityBase
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RegistrationPeriod> CreatedRegistrationPeriods { get; set; } = [];
    public ICollection<Announcement> CreatedAnnouncements { get; set; } = [];
    public ICollection<StudentScore> UpdatedStudentScores { get; set; } = [];
}
