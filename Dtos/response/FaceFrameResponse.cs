namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class FaceFrameResponse
{
    public bool Accepted { get; set; }

    public int AcceptedCount { get; set; }

    public int RequiredCount { get; set; }

    public bool IsComplete =>
        AcceptedCount >= RequiredCount;
}