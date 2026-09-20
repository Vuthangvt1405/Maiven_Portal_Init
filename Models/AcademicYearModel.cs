using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Models;

public sealed class AcademicYearModel : ModelBase
{
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public AcademicPeriodStatus Status { get; set; }
}
