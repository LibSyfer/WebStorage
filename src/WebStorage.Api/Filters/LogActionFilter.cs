using Microsoft.AspNetCore.Mvc.Filters;

namespace WebStorage.Api.Filters;

public class LogActionFilter(ILogger<LogActionFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            var actionName = context.ActionDescriptor.DisplayName;
            var parameters = string.Join(", ", context.ActionArguments.Select(kv => $"{kv.Key}: {kv.Value}"));
            logger.LogTrace("Executing action '{ActionName}' with parameters: {Parameters}", actionName, parameters);
        }

        await next();
    }
}
