namespace Maiven_Portal_Managment.Models;

public sealed class GradeComponentModel : ModelBase
{
    public long SectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
}
