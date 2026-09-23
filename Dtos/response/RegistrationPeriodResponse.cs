using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Dtos.response;

public sealed class RegistrationPeriodResponse
{
    public long Id { get; set; }
    public long SemesterId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public RegistrationPeriodStatus Status { get; set; }
    public long CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
