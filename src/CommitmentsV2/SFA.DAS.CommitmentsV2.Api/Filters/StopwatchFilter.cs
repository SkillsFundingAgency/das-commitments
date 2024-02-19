using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SFA.DAS.CommitmentsV2.Api.Filters
{
    public class StopwatchFilter : ActionFilterAttribute
    {
        private readonly ILogger<StopwatchFilter> _logger;
        private Stopwatch _stopWatch;
        private const int WarningThreshold = 5000;

        public StopwatchFilter(ILogger<StopwatchFilter> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
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
            _logger.LogWarning($"Controller action took {_stopWatch.ElapsedMilliseconds} to complete: {controllerName}.{actionName}");
        }
    }
}
