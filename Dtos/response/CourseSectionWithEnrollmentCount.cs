using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Dtos.response;

public sealed record CourseSectionWithEnrollmentCount(
    long Id,
    long CourseId,
    long SemesterId,
    long TeacherUserRoleId,
    string SectionCode,
    int Capacity,
    WeekDay DayOfWeek,
    int StartPeriod,
    int EndPeriod,
    int EnrollmentCount,
    DateOnly StartDate,
    DateOnly EndDate,
    CourseSectionStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);