using System.Diagnostics;
using System.Globalization;
using log4net;
using Maiven_Portal_Managment.Logging;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Middleware;

public sealed class RequestLoggingMiddleware(RequestDelegate next)
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(RequestLoggingMiddleware));

    public async Task InvokeAsync(HttpContext context, CurrentUserContext currentUserContext)
    {
        LogicalThreadContext.Properties["TraceId"] = context.TraceIdentifier;
        LogicalThreadContext.Properties["ActorUserId"] = "-";
        LogicalThreadContext.Properties["ActorRole"] = "-";
        var startedTimestamp = Stopwatch.GetTimestamp();

        try
        {
            await next(context);
        }
        finally
        {
            if (currentUserContext.UserId is long userId)
            {
                LogicalThreadContext.Properties["ActorUserId"] =
                    userId.ToString(CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrWhiteSpace(currentUserContext.Role))
            {
                LogicalThreadContext.Properties["ActorRole"] = currentUserContext.Role;
            }

            var elapsedMs = Stopwatch.GetElapsedTime(startedTimestamp)
                .TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture);

            Logger.Info(
                $"Method={LogFormat.FormatValue(context.Request.Method)} " +
                $"Path={LogFormat.FormatValue(context.Request.Path.Value ?? string.Empty)} " +
                $"StatusCode={context.Response.StatusCode} " +
                $"ElapsedMs={elapsedMs}");

            LogicalThreadContext.Properties.Remove("TraceId");
            LogicalThreadContext.Properties.Remove("ActorUserId");
            LogicalThreadContext.Properties.Remove("ActorRole");
        }
    }
}
