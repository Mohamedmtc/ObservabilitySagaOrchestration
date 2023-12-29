using Serilog.Context;
using System.Diagnostics;

namespace OrderService
{
    public class TraceIdMiddleware
    {
        private readonly RequestDelegate _next;

        public TraceIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
            context.Response.Headers.TryAdd("TraceId", traceId);
            using (LogContext.PushProperty("TraceId", traceId))
            {
                await _next(context);
            }
        }
    }
}
