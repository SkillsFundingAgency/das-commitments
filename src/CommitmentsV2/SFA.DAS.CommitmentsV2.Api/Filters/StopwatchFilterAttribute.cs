using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SFA.DAS.CommitmentsV2.Api.Filters;

public class StopwatchFilterAttribute(ILogger<StopwatchFilterAttribute> logger) : ActionFilterAttribute
{
    private Stopwatch _stopWatch;
    private const int WarningThreshold = 5000;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _stopWatch = new Stopwatch();
        _stopWatch.Start();
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        _stopWatch.Stop();

        if (_stopWatch.ElapsedMilliseconds <= WarningThreshold) return;

        var controllerName = context.RouteData.Values["controller"];
        var actionName = context.RouteData.Values["action"];
        logger.LogWarning("Controller action took {ElapsedMilliseconds} to complete: {controllerName}.{actionName}.", _stopWatch.ElapsedMilliseconds, controllerName, actionName);
    }
}