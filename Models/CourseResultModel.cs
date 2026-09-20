using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Models;

public sealed class CourseResultModel : ModelBase
{
    public long EnrollmentId { get; set; }
    public decimal? FinalScore { get; set; }
    public string? LetterGrade { get; set; }
    public decimal? GradePoint { get; set; }
    public ResultStatus? ResultStatus { get; set; }
}
