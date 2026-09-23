using System.ComponentModel.DataAnnotations;

namespace Maiven_Portal_Managment.Dtos.request;

public sealed record CreateCourseRequest(
    [Required] string CourseCode,
    [Required] string CourseName,
    [Required, Range(1, 10)] int Credits,
    string? Description = null
);