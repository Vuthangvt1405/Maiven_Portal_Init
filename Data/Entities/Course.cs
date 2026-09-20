using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Data.Entities;

public class Course : EntityBase
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string? Description { get; set; }
    public ActiveStatus Status { get; set; }

    public ICollection<CourseSection> CourseSections { get; set; } = [];
}
