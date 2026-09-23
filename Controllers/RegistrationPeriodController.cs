using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Services;
using Maiven_Portal_Managment.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/registrationPeriods")]
public sealed class RegistrationPeriodController(RegistrationPeriodService registrationPeriodService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin.Code)]
    public async Task<ActionResult<RegistrationPeriodResponse>> Create(
        [FromBody] CreateRegistrationPeriodRequest request,
        CurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var userId = currentUserContext.RoleUserId;
        if(userId == null)
        {
            throw new UnauthorizedException("You are not authorized to perform this action.");
        }

        var response = await registrationPeriodService.CreateAsync(request, userId.Value, cancellationToken);
        
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RegistrationPeriodResponse>>> GetAll(
        [FromQuery] RegistrationPeriodQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await registrationPeriodService.GetAllAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{registrationPeriodId:long}")]
    [Authorize(Roles = SystemRoles.Admin.Code)]
    public async Task<ActionResult<RegistrationPeriodResponse>> Update(
        [FromRoute] long registrationPeriodId,
        [FromBody] UpdateRegistrationPeriodRequest request,
        CancellationToken cancellationToken)
    {
        var response = await registrationPeriodService.UpdateAsync(registrationPeriodId, request, cancellationToken);
        return Ok(response);
    }
}
