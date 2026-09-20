using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Models;

public sealed class AnnouncementModel : ModelBase
{
    public long CreatedById { get; set; }
    public long? SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AnnouncementStatus Status { get; set; }
}
