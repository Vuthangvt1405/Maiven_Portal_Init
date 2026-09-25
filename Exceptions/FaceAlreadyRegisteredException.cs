using Microsoft.AspNetCore.Http;

namespace Maiven_Portal_Managment.Exceptions;

public sealed class FaceAlreadyRegisteredException : AppException
{
    public const string ErrorCode = "FaceAlreadyRegistered";

    public FaceAlreadyRegisteredException()
        : base(
            StatusCodes.Status409Conflict,
            "This face is already registered.",
            code: ErrorCode)
    {
    }
}
