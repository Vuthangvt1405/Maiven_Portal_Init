using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Models;

public sealed class SemesterModel : ModelBase
{
    public long AcademicYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public AcademicPeriodStatus Status { get; set; }
}
