using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Maiven_Portal_Managment.Logging;

public sealed class ControllerActionLoggingFilter(ActionLogService actionLogService)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var descriptor = (ControllerActionDescriptor)context.ActionDescriptor;
        var request = context.HttpContext.Request;
        var routeMetadata = context.RouteData.Values
            .Where(pair => pair.Key is not "controller" and not "action" && IsSafeScalar(pair.Value))
            .Select(pair => (pair.Key, pair.Value))
            .ToArray();
        var startMetadata = new List<(string Name, object? Value)>
        {
            ("HttpMethod", request.Method),
            ("Path", request.Path.Value)
        };
        startMetadata.AddRange(routeMetadata);

        using var operation = actionLogService.Begin(
            descriptor.ControllerTypeInfo.AsType(),
            "Controller",
            $"{descriptor.ControllerName}.{descriptor.ActionName}",
            startMetadata.ToArray());

        var executedContext = await next();
        if (executedContext.Exception is not null && !executedContext.ExceptionHandled)
        {
            return;
        }

        operation.Complete(("StatusCode", ResolveStatusCode(executedContext)));
    }

    private static int ResolveStatusCode(ActionExecutedContext context)
    {
        if (context.Result is IStatusCodeActionResult { StatusCode: int statusCode })
        {
            return statusCode;
        }

        return context.Result switch
        {
            ObjectResult => StatusCodes.Status200OK,
            EmptyResult => StatusCodes.Status200OK,
            _ => context.HttpContext.Response.StatusCode
        };
    }

    private static bool IsSafeScalar(object? value) =>
        value is null or string or bool or byte or sbyte or short or ushort or int or uint or long or ulong or Guid;
}
