using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Models;

public sealed class RegistrationPeriodModel : ModelBase
{
    public long SemesterId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public RegistrationPeriodStatus Status { get; set; }
    public long CreatedById { get; set; }
}
