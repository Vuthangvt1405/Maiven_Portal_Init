namespace Maiven_Portal_Managment.Data.Entities;

public class Enrollment : EntityBase
{
    public long StudentUserRoleId { get; set; }
    public long SectionId { get; set; }

    public UserRole StudentUserRole { get; set; } = null!;
    public CourseSection Section { get; set; } = null!;
    public ICollection<StudentScore> StudentScores { get; set; } = [];
    public CourseResult? CourseResult { get; set; }
}
