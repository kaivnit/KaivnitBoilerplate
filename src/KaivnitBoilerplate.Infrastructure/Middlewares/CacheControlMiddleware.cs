using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace KaivnitBoilerplate.Infrastructure.Middlewares;

public class CacheControlMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CacheControlMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public CacheControlMiddleware(
        RequestDelegate next,
        ILogger<CacheControlMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            if (!context.Response.Headers.ContainsKey("Cache-Control"))
            {
                var cacheControl = DetermineCacheControl(path);
                context.Response.Headers.Append("Cache-Control", cacheControl);
            }

            var currentCacheControl = context.Response.Headers["Cache-Control"].ToString();

            // Add Pragma for HTTP/1.0
            if (!context.Response.Headers.ContainsKey("Pragma") &&
                (currentCacheControl.Contains("no-cache") || currentCacheControl.Contains("no-store")))
            {
                context.Response.Headers.Append("Pragma", "no-cache");
            }

            // Add Expires header
            if (!context.Response.Headers.ContainsKey("Expires"))
            {
                var expires = DetermineExpires(path);
                if (!string.IsNullOrEmpty(expires))
                {
                    context.Response.Headers.Append("Expires", expires);
                }
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }

    private string DetermineCacheControl(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;

        // Sensitive endpoints - NO CACHE
        if (IsSensitiveEndpoint(pathValue))
        {
            return "no-store, no-cache, must-revalidate, private, max-age=0";
        }

        // Public static content - CACHE OK
        if (IsPublicStaticContent(pathValue))
        {
            var maxAge = _configuration.GetValue<int>("Security:PublicCacheMaxAgeSeconds", 3600);
            return $"public, max-age={maxAge}, immutable";
        }

        // Default - NO CACHE
        return "no-store, no-cache, must-revalidate, private";
    }

    private string DetermineExpires(string path)
    {
        if (IsSensitiveEndpoint(path))
        {
            return "0";
        }

        if (IsPublicStaticContent(path))
        {
            return DateTime.UtcNow.AddHours(1).ToString("R");
        }

        return "0";
    }

    private bool IsSensitiveEndpoint(string path)
    {
        var patterns = new[] { "/api/auth/", "/api/account/", "/api/user/", "/api/admin/" };
        return patterns.Any(p => path.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsPublicStaticContent(string path)
    {
        var extensions = new[] { ".js", ".css", ".jpg", ".png", ".woff", ".woff2" };
        return extensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }
}
