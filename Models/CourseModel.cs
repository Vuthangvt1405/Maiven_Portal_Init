using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Models;

public sealed class CourseModel : ModelBase
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string? Description { get; set; }
    public ActiveStatus Status { get; set; }
}
