using Microsoft.AspNetCore.Http;

namespace Maiven_Portal_Managment.Exceptions;

public sealed class InconsistentFaceFramesException : AppException
{
    public const string ErrorCode = "InconsistentFaceFrames";

    public InconsistentFaceFramesException()
        : base(
            StatusCodes.Status400BadRequest,
            "All face registration frames must belong to the same person.",
            code: ErrorCode)
    {
    }
}

