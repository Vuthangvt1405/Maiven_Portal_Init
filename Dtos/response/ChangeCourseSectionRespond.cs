namespace Maiven_Portal_Managment.Dtos.response;

public sealed class ChangeCourseSectionResponse
{
    public long SectionId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}