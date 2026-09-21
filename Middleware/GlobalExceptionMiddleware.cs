using System.Text.Json;
using log4net;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Logging;
using Maiven_Portal_Managment.Services.Security;
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

    public async Task InvokeAsync(
        HttpContext context,
        CurrentUserContext currentUserContext)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException exception) when (context.RequestAborted.IsCancellationRequested)
        {
            Logger.Warn(
                $"Request was cancelled. {BuildRequestMetadata(context, currentUserContext)}",
                exception);
            throw;
        }
        catch (Exception exception)
        {
            LogException(context, currentUserContext, exception);

            if (context.Response.HasStarted)
            {
                throw;
            }

            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private static void LogException(
        HttpContext context,
        CurrentUserContext currentUserContext,
        Exception exception)
    {
        var statusCode = exception is AppException appException
            ? appException.StatusCode
            : StatusCodes.Status500InternalServerError;
        var logMessage =
            $"Request failed. {BuildRequestMetadata(context, currentUserContext)}, " +
            $"StatusCode={statusCode}";

        if (exception is AppException)
        {
            Logger.Warn(logMessage, exception);
        }
        else
        {
            Logger.Error(logMessage, exception);
        }
    }

    private static string BuildRequestMetadata(
        HttpContext context,
        CurrentUserContext currentUserContext)
    {
        var metadata =
            $"Method={ActionLogService.FormatValue(context.Request.Method)}, " +
            $"Path={ActionLogService.FormatValue(context.Request.Path.Value ?? string.Empty)}, " +
            $"TraceId={ActionLogService.FormatValue(context.TraceIdentifier)}";

        if (currentUserContext.UserId is long userId)
        {
            metadata += $", ActorUserId={userId}";
        }

        if (!string.IsNullOrWhiteSpace(currentUserContext.Role))
        {
            metadata += $", ActorRole={ActionLogService.FormatValue(currentUserContext.Role)}";
        }

        return metadata;
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
