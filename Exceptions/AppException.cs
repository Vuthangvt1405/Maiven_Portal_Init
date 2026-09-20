namespace Maiven_Portal_Managment.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(int statusCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
