using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PaymentsAPI.Services.Metrics
{
    public class RequestProcessingTimeLoggerFilterAttribute : ActionFilterAttribute
    {
        readonly Stopwatch stopwatch = new Stopwatch();

        public override void OnActionExecuting(ActionExecutingContext context) => stopwatch.Start();

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            stopwatch.Stop();

            RequestProcessingTimeEventSource.Log.Request(
                context.HttpContext.Request.GetDisplayUrl(), stopwatch.ElapsedMilliseconds);
        }
    }
}