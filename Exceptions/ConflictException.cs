using Microsoft.AspNetCore.Http;

namespace Maiven_Portal_Managment.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(string message, Exception? innerException = null)
        : base(StatusCodes.Status409Conflict, message, innerException)
    {
    }
}
