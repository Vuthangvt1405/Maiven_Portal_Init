using System.Diagnostics;
using System.Globalization;
using System.Text;
using log4net;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Logging;

public sealed class ActionLogService(
    IHttpContextAccessor httpContextAccessor,
    CurrentUserContext currentUserContext)
{
    public ActionLogOperation Begin<TSource>(
        string layer,
        string operation,
        params (string Name, object? Value)[] metadata) =>
        Begin(typeof(TSource), layer, operation, metadata);

    public ActionLogOperation Begin(
        Type sourceType,
        string layer,
        string operation,
        params (string Name, object? Value)[] metadata)
    {
        var logger = LogManager.GetLogger(sourceType);
        var context = httpContextAccessor.HttpContext;
        var commonMetadata = new (string Name, object? Value)[]
        {
            ("TraceId", context?.TraceIdentifier),
            ("ActorUserId", currentUserContext.UserId),
            ("ActorRole", currentUserContext.Role)
        };

        logger.Info(FormatMessage(layer, operation, "Started", commonMetadata, metadata));

        return new ActionLogOperation(
            logger,
            layer,
            operation,
            Stopwatch.GetTimestamp(),
            commonMetadata);
    }

    internal static string FormatMessage(
        string layer,
        string operation,
        string phase,
        IReadOnlyList<(string Name, object? Value)> commonMetadata,
        IReadOnlyList<(string Name, object? Value)> metadata)
    {
        var message = new StringBuilder()
            .Append("Layer=").Append(FormatValue(layer))
            .Append(" Operation=").Append(FormatValue(operation))
            .Append(" Phase=").Append(FormatValue(phase));

        AppendMetadata(message, commonMetadata);
        AppendMetadata(message, metadata);
        return message.ToString();
    }

    private static void AppendMetadata(
        StringBuilder message,
        IReadOnlyList<(string Name, object? Value)> metadata)
    {
        foreach (var (name, value) in metadata)
        {
            if (value is null)
            {
                continue;
            }

            message
                .Append(' ')
                .Append(SanitizeName(name))
                .Append('=')
                .Append(FormatValue(value));
        }
    }

    private static string SanitizeName(string value)
    {
        var sanitized = new string(value
            .Where(character => char.IsLetterOrDigit(character) || character == '_')
            .ToArray());
        return string.IsNullOrEmpty(sanitized) ? "Field" : sanitized;
    }

    internal static string FormatValue(object value)
    {
        var text = value switch
        {
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        } ?? string.Empty;

        return '"' + text
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal) + '"';
    }
}

public sealed class ActionLogOperation : IDisposable
{
    private readonly ILog _logger;
    private readonly string _layer;
    private readonly string _operation;
    private readonly long _startedTimestamp;
    private readonly IReadOnlyList<(string Name, object? Value)> _commonMetadata;
    private bool _completed;

    internal ActionLogOperation(
        ILog logger,
        string layer,
        string operation,
        long startedTimestamp,
        IReadOnlyList<(string Name, object? Value)> commonMetadata)
    {
        _logger = logger;
        _layer = layer;
        _operation = operation;
        _startedTimestamp = startedTimestamp;
        _commonMetadata = commonMetadata;
    }

    public void Complete(params (string Name, object? Value)[] metadata)
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        var completionMetadata = new List<(string Name, object? Value)>(metadata)
        {
            ("ElapsedMs", Stopwatch.GetElapsedTime(_startedTimestamp).TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture))
        };

        _logger.Info(ActionLogService.FormatMessage(
            _layer,
            _operation,
            "Completed",
            _commonMetadata,
            completionMetadata));
    }

    public void Dispose()
    {
    }
}
