namespace Maiven_Portal_Managment.Dtos.response;

public sealed record EnrollmentResponse(
    long Id,
    long UserId,
    long StudentUserRoleId,
    string StudentName,
    string StudentEmail,
    long SectionId,
    string SectionCode,
    long CourseId,
    string CourseCode,
    string CourseName,
    long SemesterId,
    string SemesterName,
    long AcademicYearId,
    string AcademicYearName,
    DateTime CreatedAt,
    DateTime UpdatedAt);