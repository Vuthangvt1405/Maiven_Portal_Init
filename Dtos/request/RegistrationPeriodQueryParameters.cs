using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Dtos.request;

public sealed class RegistrationPeriodQueryParameters
{
    public long? SemesterId { get; set; }
    
    public RegistrationPeriodStatus? Status { get; set; }
}
