namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class FaceRegisterResponse
{
    public Guid SessionId { get; set; }

    public int RequiredFrames { get; set; }

    public DateTime ExpiresAt { get; set; }
}