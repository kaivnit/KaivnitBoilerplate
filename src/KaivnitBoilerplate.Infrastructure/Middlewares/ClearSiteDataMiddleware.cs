using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KaivnitBoilerplate.Infrastructure.Middlewares;

public class ClearSiteDataMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClearSiteDataMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public ClearSiteDataMiddleware(
        RequestDelegate next,
        ILogger<ClearSiteDataMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (IsLogoutEndpoint(context.Request.Path))
        {
            var enabled = _configuration.GetValue<bool>("Security:EnableClearSiteData", true);

            if (enabled && context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                context.Response.Headers.Append("Clear-Site-Data", "\"cache\", \"cookies\", \"storage\"");
                _logger.LogInformation("Clear-Site-Data header sent for logout");
            }
        }
    }

    private bool IsLogoutEndpoint(PathString path)
    {
        var logoutPaths = new[] { "/api/auth/logout", "/api/auth/revoke-token" };
        return logoutPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }
}
