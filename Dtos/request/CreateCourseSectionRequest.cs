using System.ComponentModel.DataAnnotations;
using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Dtos.request;
public sealed record CreateCourseSectionRequest(
    [Required] long CourseId,
    [Required] long SemesterId,
    [Required] long TeacherUserRoleId,
    [Required, MaxLength(50)] string SectionCode,
    [Required, Range(1, 500)] int Capacity,
    [Required, EnumDataType(typeof(WeekDay))] WeekDay DayOfWeek,
    [Required, Range(1, 10), EnumDataType(typeof(ClassPeriod))] ClassPeriod StartPeriod,
    [Required, Range(1, 10), EnumDataType(typeof(ClassPeriod))] ClassPeriod EndPeriod,
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate,
    [Required, EnumDataType(typeof(CourseSectionStatus))] CourseSectionStatus Status
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartPeriod >= EndPeriod)
        {
            yield return new ValidationResult(
                "Start period must be earlier than end period.",
                [nameof(StartPeriod), nameof(EndPeriod)] );
        }

        if (StartDate >= EndDate)
        {
            yield return new ValidationResult(
                "Start date must be earlier than or equal to end date.",
                [nameof(StartDate), nameof(EndDate) ]);
        }
    }
}