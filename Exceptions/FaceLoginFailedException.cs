using Maiven_Portal_Managment.Dtos.Response;
using Microsoft.AspNetCore.Http;

namespace Maiven_Portal_Managment.Exceptions;

public sealed class FaceLoginFailedException : AppException
{
    public const string ErrorCode = "FaceVerificationFailed";
    public const string InsufficientMatchingFramesCode = "InsufficientMatchingFrames";

    public FaceLoginFailedException(
        IReadOnlyList<FaceLoginFrameResponse> frames,
        string errorCode = ErrorCode)
        : base(
            StatusCodes.Status401Unauthorized,
            "Face verification failed.",
            code: errorCode)
    {
        Frames = frames;
    }

    public IReadOnlyList<FaceLoginFrameResponse> Frames { get; }
}
