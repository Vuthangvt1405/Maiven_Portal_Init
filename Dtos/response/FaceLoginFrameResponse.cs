using Maiven_Portal_Managment.Common;

namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class FaceLoginFrameResponse
{
    public int FrameNumber { get; init; }

    public FaceFrameStatus Status { get; init; }

    public bool Accepted => Status == FaceFrameStatus.Success;
}
