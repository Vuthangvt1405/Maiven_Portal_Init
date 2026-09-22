
namespace Maiven_Portal_Managment.Dtos.response;
public sealed record CourseSectionDetailResponse(
    long Id,
    string? SectionCode,
    long CourseId,
    long SemesterId,
    PagedResponse<StudentInCourseSectionResponse>? Students
);

public sealed record StudentInCourseSectionResponse(
    long EnrollmentId,
    long StudentUserRoleId,
    string FullName,
    string Email,
    IReadOnlyList<StudentScoreDetailResponse> Scores
);

public sealed record StudentScoreDetailResponse(
    long ComponentId,
    string ComponentName,
    decimal Weight,
    decimal? ScoreValue
);

