using Maiven_Portal_Managment.Data.Entities.Enums;

namespace Maiven_Portal_Managment.Data.Entities;

public class Announcement : EntityBase
{
    public long CreatedById { get; set; }
    public long? SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AnnouncementStatus Status { get; set; }

    public User CreatedBy { get; set; } = null!;
    public CourseSection? Section { get; set; }
}
