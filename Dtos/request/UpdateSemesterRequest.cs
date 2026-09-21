using System.ComponentModel.DataAnnotations;

namespace Maiven_Portal_Managment.Dtos.Request;

public sealed class UpdateSemesterRequest
{
    [Range(1, long.MaxValue)]
    public long AcademicYearId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateOnly? StartDate { get; set; }

    [Required]
    public DateOnly? EndDate { get; set; }
}
