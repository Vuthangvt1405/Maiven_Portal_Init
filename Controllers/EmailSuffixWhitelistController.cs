using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/email-suffix-whitelist")]
[Authorize(Roles = SystemRoles.Admin.Code)]
public sealed class EmailSuffixWhitelistController(EmailSuffixWhitelistService emailSuffixWhitelistService) : ControllerBase
{
    /// <summary>Creates an approved email suffix for public Student registration.</summary>
    [HttpPost]
    public async Task<ActionResult<EmailSuffixWhitelistRuleResponse>> Create(
        [FromBody] CreateEmailSuffixWhitelistRuleRequest request,
        CancellationToken cancellationToken)
    {
        var response = await emailSuffixWhitelistService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Retrieves all approved email suffixes for public Student registration.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmailSuffixWhitelistRuleResponse>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(await emailSuffixWhitelistService.GetAllAsync(cancellationToken));
}
