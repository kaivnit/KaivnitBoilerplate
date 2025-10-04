using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KaivnitBoilerplate.Infrastructure.Middlewares;

public class RequestTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTrackingMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public RequestTrackingMiddleware(
        RequestDelegate next,
        ILogger<RequestTrackingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var enableTracking = _configuration.GetValue<bool>("Security:EnableRequestTracking", true);

        if (enableTracking)
        {
            var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault()
                         ?? context.TraceIdentifier
                         ?? Guid.NewGuid().ToString();

            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                             ?? Guid.NewGuid().ToString();

            context.Items["RequestId"] = requestId;
            context.Items["CorrelationId"] = correlationId;
            context.Items["RequestStartTime"] = DateTimeOffset.UtcNow;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Append("X-Request-ID", requestId);
                context.Response.Headers.Append("X-Correlation-ID", correlationId);

                if (context.Items["RequestStartTime"] is DateTimeOffset startTime)
                {
                    var duration = (DateTimeOffset.UtcNow - startTime).TotalMilliseconds;
                    context.Response.Headers.Append("X-Response-Time", $"{duration:F0}ms");
                }

                return Task.CompletedTask;
            });
        }

        await _next(context);
    }
}
