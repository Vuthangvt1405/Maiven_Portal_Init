using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/enrollment")]
// [Authorize(Roles = SystemRoles.Admin.Code)]
public sealed class EnrollmentsController(EnrollmentService enrollmentService) : ControllerBase
{
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

    [HttpGet("{enrollmentId:long}")]
    public async Task<ActionResult<EnrollmentResponse>> GetById(long enrollmentId, CancellationToken cancellationToken) =>
        Ok(await enrollmentService.GetByIdAsync(enrollmentId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<EnrollmentResponse>> Create(CreateEnrollmentRequest request, CancellationToken cancellationToken)
    {
        var response = await enrollmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { enrollmentId = response.Id }, response);
    }

    [HttpPost("batch")]
    public async Task<ActionResult<EnrollmentBatchResponse>> CreateBatch(
        CreateEnrollmentsRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await enrollmentService.CreateBatchAsync(request, cancellationToken));
    }

    [HttpPut("{enrollmentId:long}")]
    public async Task<ActionResult<EnrollmentResponse>> Update(long enrollmentId, UpdateEnrollmentRequest request, CancellationToken cancellationToken) =>
        Ok(await enrollmentService.UpdateAsync(enrollmentId, request, cancellationToken));

    [HttpDelete("{enrollmentId:long}")]
    public async Task<IActionResult> Delete(long enrollmentId, CancellationToken cancellationToken)
    {
        await enrollmentService.DeleteAsync(enrollmentId, cancellationToken);
        return NoContent();
    }
}