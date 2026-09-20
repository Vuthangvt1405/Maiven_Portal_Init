using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Data.Entities;

public class CourseSection : EntityBase
{
    public long CourseId { get; set; }
    public long SemesterId { get; set; }
    public long TeacherUserRoleId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public WeekDay DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public CourseSectionStatus Status { get; set; }

    public Course Course { get; set; } = null!;
    public Semester Semester { get; set; } = null!;
    public UserRole TeacherUserRole { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Announcement> Announcements { get; set; } = [];
    public ICollection<GradeComponent> GradeComponents { get; set; } = [];
}
