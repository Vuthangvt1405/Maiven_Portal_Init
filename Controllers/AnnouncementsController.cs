using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/announcements")]
public sealed class AnnouncementsController(AnnouncementService announcementService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin.Code + "," + SystemRoles.Teacher.Code)]
    [ProducesResponseType<AnnouncementResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AnnouncementResponse>> Create(
        [FromBody] CreateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        var response = await announcementService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { announcementId = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType<PagedResponse<AnnouncementResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<AnnouncementResponse>>> GetAll(
        [FromQuery] long? sectionId,
        [FromQuery] string? title,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);
        var result = await announcementService.GetPagedAsync(
            sectionId, title, pageNumber, pageSize, cancellationToken);

        return Ok(new PagedResponse<AnnouncementResponse>
        {
            Items = result.Items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            TotalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize)
        });
    }

    [HttpGet("{announcementId:long}")]
    [ProducesResponseType<AnnouncementResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AnnouncementResponse>> GetById(
        long announcementId,
        CancellationToken cancellationToken)
    {
        return Ok(await announcementService.GetByIdAsync(announcementId, cancellationToken));
    }

    [HttpPut("{announcementId:long}")]
    [Authorize(Roles = SystemRoles.Admin.Code + "," + SystemRoles.Teacher.Code)]
    public async Task<ActionResult<AnnouncementResponse>> Update(
        long announcementId,
        [FromBody] UpdateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await announcementService.UpdateAsync(announcementId, request, cancellationToken));
    }

    // [HttpDelete("{announcementId:long}")]
    // [Authorize(Roles = SystemRoles.Admin.Code + "," + SystemRoles.Teacher.Code)]
    // public async Task<IActionResult> Delete(long announcementId, CancellationToken cancellationToken)
    // {
    //     await announcementService.DeleteAsync(announcementId, cancellationToken);
    //     return NoContent();
    // }
}