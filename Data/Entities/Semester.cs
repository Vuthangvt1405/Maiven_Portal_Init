using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Data.Entities;

public class Semester : EntityBase
{
    public long AcademicYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public AcademicPeriodStatus Status { get; set; }

    public AcademicYear AcademicYear { get; set; } = null!;
    public ICollection<CourseSection> CourseSections { get; set; } = [];
    public ICollection<RegistrationPeriod> RegistrationPeriods { get; set; } = [];
}
