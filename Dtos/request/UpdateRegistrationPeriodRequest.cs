using System.ComponentModel.DataAnnotations;
using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Dtos.request;

public sealed class UpdateRegistrationPeriodRequest
{
    [Required]
    public DateTime StartAt { get; set; }
    
    [Required]
    public DateTime EndAt { get; set; }
    
    [Required]
    public RegistrationPeriodStatus Status { get; set; }
}
