using System.ComponentModel.DataAnnotations;

namespace Maiven_Portal_Managment.Dtos.Request;

public sealed class RequestPasswordResetOtpRequest
{
    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; set; } = string.Empty;
}

public sealed class VerifyPasswordResetOtpRequest
{
    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(6, MinimumLength = 6)]
    public string Otp { get; set; } = string.Empty;
}

public sealed class ConfirmPasswordResetRequest
{
    [Required]
    public string ResetToken { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}
