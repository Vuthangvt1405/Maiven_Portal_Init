using System.ComponentModel.DataAnnotations;

namespace Maiven_Portal_Managment.Dtos.request;

public sealed record CreateAnnouncementRequest
{
    public long? SectionId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;
}