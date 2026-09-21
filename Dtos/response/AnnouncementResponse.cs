namespace Maiven_Portal_Managment.Dtos.response;

public sealed class AnnouncementResponse
{
    public long Id { get; set; }
    public long CreatedById { get; set; }
    public long? SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}