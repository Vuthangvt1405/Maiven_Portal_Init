using Microsoft.AspNetCore.Http;

namespace Maiven_Portal_Managment.Exceptions;

public sealed class BadRequestException : AppException
{
    public BadRequestException(string message, Exception? innerException = null)
        : base(StatusCodes.Status400BadRequest, message, innerException)
    {
    }
}
