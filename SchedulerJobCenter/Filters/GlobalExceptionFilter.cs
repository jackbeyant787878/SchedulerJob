using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SchedulerJobCenter.Shared;
namespace SchedulerJobCenter.Api.Filters
{
    public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;

            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            var (statusCode, message) = ex switch
            {
                ArgumentException or InvalidOperationException =>
                    (StatusCodes.Status400BadRequest, ex.Message),
                KeyNotFoundException =>
                    (StatusCodes.Status404NotFound, ex.Message),
                OperationCanceledException =>
                    (StatusCodes.Status499ClientClosedRequest, "Request was cancelled."),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            context.Result = new ObjectResult(ApiResponse.Fail(message))
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
        }
    }
}
