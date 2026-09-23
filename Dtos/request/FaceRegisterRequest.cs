using System.ComponentModel.DataAnnotations;

namespace Maiven_Portal_Managment.Dtos.Request;

public sealed class FaceRegisterRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;
}