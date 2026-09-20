using Microsoft.AspNetCore.Http;

namespace Maiven_Portal_Managment.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message, Exception? innerException = null)
        : base(StatusCodes.Status404NotFound, message, innerException)
    {
    }
}
