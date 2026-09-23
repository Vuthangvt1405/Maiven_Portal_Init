using System.ComponentModel.DataAnnotations;

namespace Maiven_Portal_Managment.Dtos.Request;

public sealed class DeleteFaceRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;
}