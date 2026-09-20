using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Data.Entities;

public class AcademicYear : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public AcademicPeriodStatus Status { get; set; }

    public ICollection<Semester> Semesters { get; set; } = [];
}
