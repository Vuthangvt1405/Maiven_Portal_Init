using System.ComponentModel.DataAnnotations;
using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Dtos.request;

public sealed record UpdateCourseSectionRequest
{
    [Required]
    public long? TeacherUserRoleId { get; set; }
    [Required]
    public long? SemesterId { get; set; }
    [Required]
    [Range(1, 500)]
    public int Capacity { get; set; }

    [Required]
    [EnumDataType(typeof(WeekDay))]
    public WeekDay DayOfWeek { get; set; }

    [Required]
    [Range(1, 10)]
    [EnumDataType(typeof(ClassPeriod))]
    public ClassPeriod StartPeriod { get; set; }

    [Required]
    [Range(1, 10)]
    [EnumDataType(typeof(ClassPeriod))]
    public ClassPeriod EndPeriod { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required]
    [EnumDataType(typeof(CourseSectionStatus))]
    public CourseSectionStatus Status { get; set; }
}