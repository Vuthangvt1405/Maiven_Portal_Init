using System.ComponentModel.DataAnnotations;

namespace Maiven_Portal_Managment.Dtos.Request;

public sealed class CreateEmailSuffixWhitelistRuleRequest
{
    [Required]
    [MaxLength(253)]
    public string Suffix { get; init; } = string.Empty;
}
