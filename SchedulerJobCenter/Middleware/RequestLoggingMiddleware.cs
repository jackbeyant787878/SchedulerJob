using System.Diagnostics;

namespace SchedulerJobCenter.Api.Middleware
{
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await next(context);
                sw.Stop();
                logger.LogInformation(
                    "{Method} {Path} => {StatusCode} ({ElapsedMs}ms)",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds);
            }
            catch (Exception)
            {
                sw.Stop();
                logger.LogError("Request failed: {Method} {Path} ({ElapsedMs}ms)",
                    context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
