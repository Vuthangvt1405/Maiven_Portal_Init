
namespace Maiven_Portal_Managment.Dtos.response;
public sealed record CourseSectionDetailResponse(
    long Id,
    string? SectionCode,
    CourseResponse Course,
    long SemesterId,
    PagedResponse<StudentInCourseSectionResponse>? Students
);

public sealed record StudentInCourseSectionResponse(
    long EnrollmentId,
    long StudentUserRoleId,
    string FullName,
    string Email,
    IReadOnlyList<StudentScoreDetailResponse> Scores,
    StudentFinalResultResponse? FinalResult
);

public sealed record StudentScoreDetailResponse(
    long ScoreId,
    long ComponentId,
    string ComponentName,
    decimal Weight,
    decimal? ScoreValue
);

