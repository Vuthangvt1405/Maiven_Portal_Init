using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class EmailSuffixWhitelistRepository(AppDbContext dbContext)
{
    public Task<bool> HasMatchingSuffixAsync(string emailDomain, CancellationToken cancellationToken) =>
        dbContext.EmailSuffixWhitelistRules.AnyAsync(
            rule => emailDomain == rule.Suffix ||
                EF.Functions.Like(emailDomain, "%." + rule.Suffix),
            cancellationToken);

    public Task<bool> ExistsAsync(string suffix, CancellationToken cancellationToken) =>
        dbContext.EmailSuffixWhitelistRules.AnyAsync(rule => rule.Suffix == suffix, cancellationToken);

    public async Task<EmailSuffixWhitelistRule> AddAsync(
        EmailSuffixWhitelistRule rule,
        CancellationToken cancellationToken)
    {
        dbContext.EmailSuffixWhitelistRules.Add(rule);
        await dbContext.SaveChangesAsync(cancellationToken);
        return rule;
    }

    public async Task<IReadOnlyList<EmailSuffixWhitelistRule>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.EmailSuffixWhitelistRules
            .AsNoTracking()
            .OrderBy(rule => rule.Suffix)
            .ToListAsync(cancellationToken);

    public async Task<bool> DeleteAsync(long ruleId, CancellationToken cancellationToken)
    {
        var rule = await dbContext.EmailSuffixWhitelistRules
            .SingleOrDefaultAsync(rule => rule.Id == ruleId, cancellationToken);
        if (rule is null)
        {
            return false;
        }

        rule.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
