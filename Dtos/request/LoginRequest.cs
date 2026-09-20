using System.ComponentModel.DataAnnotations;

namespace Maiven_Portal_Managment.Dtos.Request;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
