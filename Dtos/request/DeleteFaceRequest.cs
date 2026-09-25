using System.ComponentModel.DataAnnotations;

namespace Maiven_Portal_Managment.Dtos.Request;

public sealed class DeleteFaceRequest
{
    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
