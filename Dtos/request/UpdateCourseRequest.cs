using System.ComponentModel.DataAnnotations;
using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Dtos.request;

public sealed record UpdateCourseRequest(
    [Required] string CourseName,
    [Required, Range(1, 10)] int Credits,
    [Required, EnumDataType(typeof(ActiveStatus))] ActiveStatus Status,
    string? Description = null
);