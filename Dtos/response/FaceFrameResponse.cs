namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class FaceFrameResponse
{
    public bool Accepted { get; set; }

    public int AcceptedCount { get; set; }

    public int RequiredCount { get; set; }

    // Enough frames have been accepted; the user must still confirm before saving.
    public bool IsComplete =>
        RequiredCount > 0 && AcceptedCount >= RequiredCount;
}
