namespace Maiven_Portal_Managment.Data.Entities;

public sealed class EmailSuffixWhitelistRule : EntityBase
{
    public string Suffix { get; set; } = string.Empty;
}
