namespace Maiven_Portal_Managment.Data.Entities;

public class GradeComponent : EntityBase
{
    public long SectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }

    public CourseSection Section { get; set; } = null!;
    public ICollection<StudentScore> StudentScores { get; set; } = [];
}
