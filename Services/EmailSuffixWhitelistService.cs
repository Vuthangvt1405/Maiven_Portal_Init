using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Repository;

namespace Maiven_Portal_Managment.Services;

public sealed class EmailSuffixWhitelistService(EmailSuffixWhitelistRepository emailSuffixWhitelistRepository)
{
    public async Task<EmailSuffixWhitelistRuleResponse> CreateAsync(
        CreateEmailSuffixWhitelistRuleRequest request,
        CancellationToken cancellationToken)
    {
        var suffix = NormalizeSuffix(request.Suffix);
        if (await emailSuffixWhitelistRepository.ExistsAsync(suffix, cancellationToken))
        {
            throw new ConflictException("This email suffix is already whitelisted.");
        }

        var created = await emailSuffixWhitelistRepository.AddAsync(
            new EmailSuffixWhitelistRule { Suffix = suffix },
            cancellationToken);

        return ToResponse(created);
    }

    public async Task<IReadOnlyList<EmailSuffixWhitelistRuleResponse>> GetAllAsync(
        CancellationToken cancellationToken) =>
        (await emailSuffixWhitelistRepository.GetAllAsync(cancellationToken))
            .Select(ToResponse)
            .ToArray();

    public Task<bool> IsEmailAllowedAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var atIndex = normalizedEmail.LastIndexOf('@');
        if (atIndex <= 0 || atIndex == normalizedEmail.Length - 1)
        {
            return Task.FromResult(false);
        }

        var emailDomain = normalizedEmail[(atIndex + 1)..].TrimEnd('.');
        return emailSuffixWhitelistRepository.HasMatchingSuffixAsync(emailDomain, cancellationToken);
    }

    private static EmailSuffixWhitelistRuleResponse ToResponse(EmailSuffixWhitelistRule rule) =>
        new(rule.Id, rule.Suffix, rule.CreatedAt, rule.UpdatedAt);

    private static string NormalizeSuffix(string? value)
    {
        var suffix = value?.Trim().TrimStart('@').Trim('.').ToLowerInvariant() ?? string.Empty;
        if (suffix.Length is < 1 or > 253 || Uri.CheckHostName(suffix) != UriHostNameType.Dns)
        {
            throw new BadRequestException("Email suffix must be a valid domain, such as maiven.vn.");
        }

        return suffix;
    }
}
