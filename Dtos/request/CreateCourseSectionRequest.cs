using System.ComponentModel.DataAnnotations;
using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Dtos.request;

public sealed record CreateCourseSectionRequest
{
    [Required]
    public long CourseId { get; set; }

    [Required]
    public long SemesterId { get; set; }

    [Required]
    public long TeacherUserRoleId { get; set; }

    [Required]
    [MaxLength(50)]
    public string SectionCode { get; set; } = string.Empty;

    [Required]
    [Range(1, 500)]
    public int Capacity { get; set; }

    [Required]
    [EnumDataType(typeof(WeekDay))]
    public WeekDay DayOfWeek { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required]
    [EnumDataType(typeof(CourseSectionStatus))]
    public CourseSectionStatus Status { get; set; }
}