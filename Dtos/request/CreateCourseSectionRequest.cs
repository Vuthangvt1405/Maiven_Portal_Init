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
    [Required] int StartPeriod,
    [Required] int EndPeriod,
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate,
    [Required, EnumDataType(typeof(CourseSectionStatus))] CourseSectionStatus Status
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(typeof(ClassPeriod), StartPeriod))
        {
            yield return new ValidationResult(
                $"Start period {StartPeriod} is not a valid class period.",
                [nameof(StartPeriod)]);
        }

        if (!Enum.IsDefined(typeof(ClassPeriod), EndPeriod))
        {
            yield return new ValidationResult(
                $"End period {EndPeriod} is not a valid class period.",
                [nameof(EndPeriod)]);
        }

        if (StartPeriod >= EndPeriod)
        {
            yield return new ValidationResult(
                "Start period must be earlier than end period.",
                [nameof(StartPeriod), nameof(EndPeriod)]);
        }

        if (StartDate >= EndDate)
        {
            yield return new ValidationResult(
                "Start date must be earlier than or equal to end date.",
                [nameof(StartDate), nameof(EndDate)]);
        }
    }
}