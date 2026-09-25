using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Services;
using Maiven_Portal_Managment.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/enrollment")]
[Authorize]
public sealed class EnrollmentsController(
    EnrollmentService enrollmentService,
    CurrentUserContext currentUserContext) : ControllerBase
{
    /// <summary>Retrieves a paginated list of enrollments.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<EnrollmentResponse>>> GetAll(
        [FromQuery] EnrollmentQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await enrollmentService.GetPagedAsync(parameters, cancellationToken);
        var page = parameters.Page < 1 ? 1 : parameters.Page;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);
        return Ok(new PagedResponse<EnrollmentResponse>
        {
            Items = result.Items, PageNumber = page, PageSize = pageSize,
            TotalItems = result.TotalItems, TotalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize)
        });
    }

    /// <summary>Retrieves an enrollment by ID.</summary>
    [HttpGet("{enrollmentId:long}")]
    public async Task<ActionResult<EnrollmentResponse>> GetById(long enrollmentId, CancellationToken cancellationToken) =>
        Ok(await enrollmentService.GetByIdAsync(enrollmentId, cancellationToken));

    /// <summary>Creates an enrollment.</summary>
    [HttpPost]
    public async Task<ActionResult<EnrollmentResponse>> Create(CreateEnrollmentRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.UserId
            ?? throw new UnauthorizedException("The authenticated user could not be identified.");
        var response = await enrollmentService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { enrollmentId = response.Id }, response);
    }

    /// <summary>Creates multiple enrollments in one request.</summary>
    [HttpPost("batch")]
    public async Task<ActionResult<EnrollmentBatchResponse>> CreateBatch(
        CreateEnrollmentsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserContext.UserId
            ?? throw new UnauthorizedException("The authenticated user could not be identified.");
        return Ok(await enrollmentService.CreateBatchAsync(userId, request, cancellationToken));
    }

    /// <summary>Updates an enrollment.</summary>
    [HttpPut("{enrollmentId:long}")]
    public async Task<ActionResult<EnrollmentResponse>> Update(long enrollmentId, UpdateEnrollmentRequest request, CancellationToken cancellationToken) =>
        Ok(await enrollmentService.UpdateAsync(enrollmentId, request, cancellationToken));

    /// <summary>Deletes the authenticated student's enrollment from a course section.</summary>
    [HttpDelete("{sectionId:long}")]
    public async Task<IActionResult> Delete(long sectionId, CancellationToken cancellationToken)
    {
        var studentUserRoleId = currentUserContext.RoleUserId
            ?? throw new UnauthorizedException("The authenticated user could not be identified.");

        await enrollmentService.DeleteAsync(studentUserRoleId, sectionId, cancellationToken);
        return NoContent();
    }
}
