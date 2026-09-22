using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Dtos.response;

public sealed record CourseResponse(
    long Id,
    string CourseCode,
    string CourseName,
    int Credits,
    string? Description,
    ActiveStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);