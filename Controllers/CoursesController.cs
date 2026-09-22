using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController(CourseService courseService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin.Code)]
    public async Task<ActionResult<CourseResponse>> Create(
        [FromBody] CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var response = await courseService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { courseId = response.Id },
            response);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<CourseResponse>>> GetAll(
        [FromQuery] CourseQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await courseService.GetPagedAsync(parameters, cancellationToken);

        var pageNumber = parameters.PageNumber < 1
            ? 1
            : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1
            ? 10
            : Math.Min(parameters.PageSize, 100);
        var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

        var response = new PagedResponse<CourseResponse>
        {
            Items = result.Items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            TotalPages = totalPages
        };

        return Ok(response);
    }

    [HttpGet("{courseId:long}")]
    public async Task<ActionResult<CourseResponse>> GetById(
        long courseId,
        CancellationToken cancellationToken)
    {
        var response = await courseService.GetByIdAsync(courseId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{courseId:long}")]
    [Authorize(Roles = SystemRoles.Admin.Code)]
    public async Task<ActionResult<CourseResponse>> Update(
        long courseId,
        [FromBody] UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var response = await courseService.UpdateAsync(
            courseId,
            request,
            cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{courseId:long}")]
    [Authorize(Roles = SystemRoles.Admin.Code)]
    public async Task<IActionResult> Delete(
        long courseId,
        CancellationToken cancellationToken)
    {
        await courseService.DeleteAsync(courseId, cancellationToken);
        return NoContent();
    }
}
