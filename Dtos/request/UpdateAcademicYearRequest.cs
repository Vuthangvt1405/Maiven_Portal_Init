using System.ComponentModel.DataAnnotations;
using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Dtos.Request;

public sealed class UpdateAcademicYearRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateOnly? StartDate { get; set; }

    [Required]
    public DateOnly? EndDate { get; set; }

    [Required]
    [EnumDataType(typeof(AcademicPeriodStatus))]
    public AcademicPeriodStatus? Status { get; set; }
}
