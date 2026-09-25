namespace Maiven_Portal_Managment.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(
        int statusCode,
        string message,
        Exception? innerException = null,
        string? code = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Code = code;
    }

    public int StatusCode { get; }

    public string? Code { get; }
}
