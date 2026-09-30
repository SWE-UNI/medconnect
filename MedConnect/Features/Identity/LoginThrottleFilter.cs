using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MedConnect.Features.Identity;

[AttributeUsage(AttributeTargets.Method)]
public sealed class LoginThrottleAttribute : ServiceFilterAttribute
{
    public LoginThrottleAttribute() : base(typeof(LoginThrottleFilter))
    {
    }
}

public class LoginThrottleFilter(LoginRateLimiter limiter) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ip = context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? context.HttpContext.Connection.RemoteIpAddress?.ToString()
                 ?? "unknown";

        var (allowed, retryAfter) = limiter.TryAcquire(ip);
        if (!allowed)
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
            context.Result = new ObjectResult(new { error = "Too many attempts. Please try again after a few minutes." })
            {
                StatusCode = StatusCodes.Status429TooManyRequests
            };
            return;
        }

        await next();
    }
}