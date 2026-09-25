namespace Maiven_Portal_Managment.Dtos.Response;

public sealed record EmailSuffixWhitelistRuleResponse(
    long Id,
    string Suffix,
    DateTime CreatedAt,
    DateTime UpdatedAt);
