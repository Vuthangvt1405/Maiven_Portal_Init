using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Data.Entities;

public class CourseResult : EntityBase
{
    public long EnrollmentId { get; set; }
    public decimal? FinalScore { get; set; }
    public string? LetterGrade { get; set; }
    public decimal? GradePoint { get; set; }
    public ResultStatus? ResultStatus { get; set; }

    public Enrollment Enrollment { get; set; } = null!;
}
