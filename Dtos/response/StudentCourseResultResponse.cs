using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Dtos.response;

public sealed record StudentCourseResultResponse
{
    public long EnrollmentId { get; init; }
    public long SectionId { get; init; }
    public string SectionCode { get; init; } = string.Empty;
    public long CourseId { get; init; }
    public string CourseCode { get; init; } = string.Empty;
    public string CourseName { get; init; } = string.Empty;
    public int Credits { get; init; }
    public long SemesterId { get; init; }
    public string SemesterName { get; init; } = string.Empty;
    public long AcademicYearId { get; init; }
    public string AcademicYearName { get; init; } = string.Empty;
    public IReadOnlyList<StudentComponentScoreResponse> ComponentScores { get; init; } = [];
    public StudentFinalResultResponse? FinalResult { get; init; }
}

public sealed record StudentComponentScoreResponse
{
    public long ComponentId { get; init; }
    public string ComponentName { get; init; } = string.Empty;
    public decimal Weight { get; init; }
    public decimal? Score { get; init; }
}

public sealed record StudentFinalResultResponse
{
    public decimal? FinalScore { get; init; }
    public string? LetterGrade { get; init; }
    public decimal? GradePoint { get; init; }
    public ResultStatus? ResultStatus { get; init; }
}
