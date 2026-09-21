using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Dtos.response;

public sealed record CourseSectionResponse
{
    public long Id { get; set; }
    public long CourseId { get; set; }
    public long SemesterId { get; set; }
    public long TeacherUserRoleId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public WeekDay DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public CourseSectionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}