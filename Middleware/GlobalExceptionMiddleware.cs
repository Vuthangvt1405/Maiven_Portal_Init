using System.Text.Json;
using log4net;
using Maiven_Portal_Managment.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Maiven_Portal_Managment.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private const string UnexpectedErrorMessage = "An unexpected error occurred.";
    private static readonly ILog Logger = LogManager.GetLogger(typeof(GlobalExceptionMiddleware));
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException exception) when (context.RequestAborted.IsCancellationRequested)
        {
            Logger.Warn(
                $"Request was cancelled. Method={context.Request.Method}, Path={context.Request.Path}, TraceId={context.TraceIdentifier}",
                exception);
            throw;
        }
        catch (Exception exception)
        {
            LogException(context, exception);

            if (context.Response.HasStarted)
            {
                throw;
            }

            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private static void LogException(HttpContext context, Exception exception)
    {
        var statusCode = exception is AppException appException
            ? appException.StatusCode
            : StatusCodes.Status500InternalServerError;
        var logMessage =
            $"Request failed. Method={context.Request.Method}, Path={context.Request.Path}, " +
            $"StatusCode={statusCode}, TraceId={context.TraceIdentifier}";

        if (exception is AppException)
        {
            Logger.Warn(logMessage, exception);
        }
        else
        {
            Logger.Error(logMessage, exception);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        var appException = exception as AppException;
        var statusCode = appException?.StatusCode ?? StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Type = "about:blank",
            Title = GetTitle(statusCode),
            Status = statusCode,
            Detail = appException?.Message ?? UnexpectedErrorMessage
        };
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problemDetails,
            JsonOptions,
            CancellationToken.None);
    }

    private static string GetTitle(int statusCode)
    {
        var reasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode);
        return string.IsNullOrWhiteSpace(reasonPhrase) ? "Request Failed" : reasonPhrase;
    }
}
