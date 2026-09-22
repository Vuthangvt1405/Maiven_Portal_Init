using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Dtos.request;

public sealed record CourseSectionQueryParameters
{
    public long? CourseId { get; set; }
    public long? SemesterId { get; set; }
    public long? TeacherUserRoleId { get; set; }
    public string? SectionCode { get; set; }
    public WeekDay? DayOfWeek { get; set; }
    public CourseSectionStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}