using Microsoft.AspNetCore.Http;

namespace Maiven_Portal_Managment.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message, Exception? innerException = null)
        : base(StatusCodes.Status401Unauthorized, message, innerException)
    {
    }
}
