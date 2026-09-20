using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Data.Entities;

public class RegistrationPeriod : EntityBase
{
    public long SemesterId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public RegistrationPeriodStatus Status { get; set; }
    public long CreatedById { get; set; }

    public Semester Semester { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
