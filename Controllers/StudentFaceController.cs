using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/students/face")]
[Authorize(Roles = SystemRoles.Student.Code)]
public sealed class StudentFaceController(
    FaceService faceService,
    StudentService studentService) : ControllerBase
{
    [HttpGet("status")]
    public async Task<ActionResult<FaceStatusResponse>> GetFaceStatus(
        CurrentUserContext currentStudent,
        CancellationToken cancellationToken)
    {
        if (currentStudent.RoleUserId is not long studentId)
        {
            throw new UnauthorizedException("User ID is not available.");
        }

        var response = await faceService.GetFaceStatusAsync(studentId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<FaceRegisterResponse>> RegisterFace(
        CurrentUserContext currentStudent,
        [FromBody] FaceRegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (currentStudent.RoleUserId is not long studentId)
        {
            throw new UnauthorizedException("User ID is not available.");
        }

        var response = await faceService.RegisterFaceAsync(studentId, request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("register/{sessionId:guid}/frames")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FaceFrameResponse>> UploadFaceFrame(
        [FromRoute] Guid sessionId,
        IFormFile frame,
        CancellationToken cancellationToken)
    {
        var response = await faceService.UploadFaceFrameAsync(sessionId, frame, cancellationToken);
        return Ok(response);
    }
    
    [HttpPost("register/{sessionId:guid}/verify")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FaceVerifyResponse>> VerifyFace(
        [FromRoute] Guid sessionId,
        CancellationToken cancellationToken)
    {
        var response = await faceService.VerifyFaceAsync(sessionId, frame, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("register/{sessionId:guid}")]
    public async Task<ActionResult> CancelFaceRegistration(
        [FromRoute] Guid sessionId,
        CancellationToken cancellationToken)
    {
        await faceService.CancelFaceRegistrationAsync(sessionId, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteFaceRequest(
        CurrentUserContext currentStudent,
        CancellationToken cancellationToken)
    {
        if (currentStudent.RoleUserId is not long studentId)
        {
            throw new UnauthorizedException("User ID is not available.");
        }

        await faceService.DeleteFaceAsync(studentId, cancellationToken);
        return NoContent();
    }

}