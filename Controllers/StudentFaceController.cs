using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/student/face")]
[Authorize(Roles = SystemRoles.Student.Code)]
public sealed class StudentFaceController(FaceCredentialService faceCredentialService) : ControllerBase
{
    [HttpGet("status")]
    public async Task<ActionResult<FaceStatusResponse>> GetFaceStatus(
        CancellationToken cancellationToken)
    {
        var response = await faceCredentialService.GetFaceStatusAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("register/start")]
    public async Task<ActionResult<FaceRegisterResponse>> StartFaceRegistration(
        [FromBody] FaceRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await faceCredentialService.StartFaceRegistrationAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("register/{sessionId:guid}/frames")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FaceFrameResponse>> UploadFaceFrame(
        [FromRoute] Guid sessionId,
        IFormFile frame,
        CancellationToken cancellationToken)
    {
        var response = await faceCredentialService.UploadFaceFrameAsync(sessionId, frame, cancellationToken);
        return Ok(response);
    }

    [HttpPost("register/{sessionId:guid}/confirm")]
    public async Task<ActionResult<FaceVerifyResponse>> ConfirmFaceRegistration(
        [FromRoute] Guid sessionId,
        CancellationToken cancellationToken)
    {
        var response = await faceCredentialService.ConfirmFaceRegistrationAsync(sessionId, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("register/{sessionId:guid}")]
    public async Task<ActionResult> CancelFaceRegistration(
        [FromRoute] Guid sessionId,
        CancellationToken cancellationToken)
    {
        await faceCredentialService.CancelFaceRegistrationAsync(sessionId, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteFace(
        [FromBody] DeleteFaceRequest request,
        CancellationToken cancellationToken)
    {
        await faceCredentialService.DeleteFaceAsync(request, cancellationToken);
        return NoContent();
    }
}
