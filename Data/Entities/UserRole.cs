using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Data.Entities;

public class UserRole : EntityBase
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public ActiveStatus Status { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public ICollection<CourseSection> TaughtCourseSections { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
