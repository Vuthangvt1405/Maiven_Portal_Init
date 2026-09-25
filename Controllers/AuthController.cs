using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AuthService authService,
    PasswordResetService passwordResetService) : ControllerBase
{
    /// <summary>Registers a new student account.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.RegisterStudentAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Authenticates a user and returns an access token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>Authenticates an administrator and returns an access token.</summary>
    [HttpPost("admin/login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> LoginAdmin(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.LoginAdminAsync(request, cancellationToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("face-login")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AuthResponse>> FaceLogin(
        [FromForm] List<IFormFile> request,
        CancellationToken cancellationToken)
    {
        var response = await authService.FaceLoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>Sends a one-time password reset code when the email belongs to an account.</summary>
    [AllowAnonymous]
    [HttpPost("password-reset/otp/request")]
    public async Task<ActionResult> RequestPasswordResetOtp(
        [FromBody] RequestPasswordResetOtpRequest request,
        CancellationToken cancellationToken)
    {
        await passwordResetService.RequestOtpAsync(request, cancellationToken);
        return Accepted();
    }

    /// <summary>Verifies a password reset code and returns a short-lived reset token.</summary>
    [AllowAnonymous]
    [HttpPost("password-reset/otp/verify")]
    public async Task<ActionResult<PasswordResetTokenResponse>> VerifyPasswordResetOtp(
        [FromBody] VerifyPasswordResetOtpRequest request,
        CancellationToken cancellationToken)
    {
        var response = await passwordResetService.VerifyOtpAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>Verifies a registered Student face and returns a short-lived password reset token.</summary>
    [AllowAnonymous]
    [HttpPost("password-reset/face/verify")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PasswordResetTokenResponse>> VerifyPasswordResetFace(
        [FromForm] List<IFormFile> frames,
        CancellationToken cancellationToken)
    {
        var faceLogin = await authService.FaceLoginAsync(frames, cancellationToken);
        var response = await passwordResetService.CreateFaceResetTokenAsync(faceLogin.User.Id, cancellationToken);
        return Ok(response);
    }

    /// <summary>Changes a password using a verified, one-time reset token.</summary>
    [AllowAnonymous]
    [HttpPost("password-reset/confirm")]
    public async Task<ActionResult> ConfirmPasswordReset(
        [FromBody] ConfirmPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        await passwordResetService.ConfirmAsync(request, cancellationToken);
        return NoContent();
    }
}
