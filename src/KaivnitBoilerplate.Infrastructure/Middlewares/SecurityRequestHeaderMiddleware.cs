using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace KaivnitBoilerplate.Infrastructure.Middlewares;

public class SecurityRequestHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityRequestHeaderMiddleware> _logger;
    private readonly IConfiguration _configuration;

    // Dangerous headers that could be used for attacks
    // Note: Azure App Service uses X-Forwarded-*, X-ARR-*, X-WAWS-* headers - these are safe
    private static readonly string[] DangerousHeaders = new[]
    {
        "X-Original-URL",
        "X-Rewrite-URL",
        "X-Host",
        "Proxy-Host",
        "Proxy-Connection",
        "X-ProxyUser-Ip",
    };

    // Azure App Service safe headers - exempt from dangerous header check
    private static readonly string[] AzureSafeHeaders = new[]
    {
        "X-ARR-SSL",
        "X-ARR-ClientCert",
        "X-ARR-LOG-ID",
        "X-WAWS-Unencoded-URL",
        "X-AppService-Proto",
        "X-Site-Deployment-Id",
        "X-Original-Proto",
        "DISGUISED-HOST",
        "WAS-DEFAULT-HOSTNAME"
    };

    private static readonly char[] DangerousCharacters = new[] { '\r', '\n', '\0' };

    public SecurityRequestHeaderMiddleware(
        RequestDelegate next,
        ILogger<SecurityRequestHeaderMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // TEMPORARY: Log all request headers for debugging Azure deployment
        var isDevelopment = context.Request.Host.Host.Contains("localhost") ||
                           context.Request.Host.Host.Contains("127.0.0.1");

        if (!isDevelopment)
        {
            _logger.LogInformation("SECURITY DEBUG: Incoming request to {Path} from {Host}",
                context.Request.Path, context.Request.Host);
            foreach (var header in context.Request.Headers)
            {
                _logger.LogInformation("SECURITY DEBUG: Header {Name} = {Value}",
                    header.Key, header.Value.ToString());
            }
        }

        if (!ValidateRequestHeaders(context, out string? rejectionReason))
        {
            _logger.LogError("SECURITY: Request blocked - {Reason} for {Path}", rejectionReason, context.Request.Path);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync($"Invalid request headers: {rejectionReason}");
            return;
        }

        await _next(context);
    }

    private bool ValidateRequestHeaders(HttpContext context, out string? rejectionReason)
    {
        rejectionReason = null;
        var maxHeaderSize = _configuration.GetValue<int>("Security:MaxRequestHeaderSize", 8192);
        var blockDangerousHeaders = _configuration.GetValue<bool>("Security:BlockDangerousRequestHeaders", true);
        var validateHeaderValues = _configuration.GetValue<bool>("Security:ValidateRequestHeaderValues", true);

        foreach (var header in context.Request.Headers)
        {
            var headerName = header.Key;
            var headerValue = header.Value.ToString();

            // Check for dangerous headers
            if (blockDangerousHeaders && IsDangerousHeader(headerName))
            {
                rejectionReason = $"Dangerous header: {headerName}";
                _logger.LogWarning("SECURITY: Blocked dangerous header: {HeaderName}", headerName);
                return false;
            }

            // Check header size limit
            if (headerValue.Length > maxHeaderSize)
            {
                rejectionReason = $"Header {headerName} exceeds max size ({headerValue.Length} > {maxHeaderSize})";
                _logger.LogWarning("SECURITY: Header {HeaderName} exceeds max size: {Size} > {MaxSize}",
                    headerName, headerValue.Length, maxHeaderSize);
                return false;
            }

            // Check for CRLF injection
            if (validateHeaderValues && ContainsDangerousCharacters(headerValue))
            {
                rejectionReason = $"CRLF injection in header: {headerName}";
                _logger.LogWarning("SECURITY: CRLF injection detected in {HeaderName}", headerName);
                return false;
            }

            // Check for SQL injection (only for custom headers, not standard browser headers)
            if (validateHeaderValues && IsCustomHeader(headerName) && ContainsSqlInjectionPatterns(headerValue))
            {
                rejectionReason = $"SQL injection pattern in header: {headerName}";
                _logger.LogWarning("SECURITY: SQL injection detected in {HeaderName}", headerName);
                return false;
            }

            // Check for XSS (only for custom headers, not standard browser headers)
            if (validateHeaderValues && IsCustomHeader(headerName) && ContainsXssPatterns(headerValue))
            {
                rejectionReason = $"XSS pattern in header: {headerName}";
                _logger.LogWarning("SECURITY: XSS detected in {HeaderName}", headerName);
                return false;
            }
        }

        return true;
    }

    private bool IsDangerousHeader(string headerName)
    {
        // Skip Azure safe headers
        if (AzureSafeHeaders.Any(h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Skip headers starting with X-ARR- or X-WAWS- (Azure specific)
        if (headerName.StartsWith("X-ARR-", StringComparison.OrdinalIgnoreCase) ||
            headerName.StartsWith("X-WAWS-", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return DangerousHeaders.Any(h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsCustomHeader(string headerName)
    {
        // Standard browser headers that should not be checked for XSS/SQL injection
        var standardHeaders = new[]
        {
            "Accept", "Accept-Charset", "Accept-Encoding", "Accept-Language",
            "Authorization", "Cache-Control", "Connection", "Content-Length",
            "Content-Type", "Cookie", "Host", "If-Modified-Since",
            "If-None-Match", "If-Range", "If-Unmodified-Since", "Max-Forwards",
            "Pragma", "Proxy-Authorization", "Range", "Referer", "TE",
            "Upgrade", "User-Agent", "Via", "Warning", "X-Forwarded-For",
            "X-Forwarded-Host", "X-Forwarded-Proto", "X-Forwarded-Server",
            "X-Real-IP", "X-Request-ID", "X-Correlation-ID"
        };

        return !standardHeaders.Any(h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase));
    }

    private bool ContainsDangerousCharacters(string value)
    {
        return value.IndexOfAny(DangerousCharacters) >= 0;
    }

    private bool ContainsSqlInjectionPatterns(string value)
    {
        var patterns = new[] { "' OR '1'='1", "'; DROP TABLE", "UNION SELECT" };
        return patterns.Any(p => value.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private bool ContainsXssPatterns(string value)
    {
        // Only check for obvious XSS patterns, be more lenient with common browser headers
        var patterns = new[] { "<script>", "javascript:", "onerror=", "<iframe>" };
        return patterns.Any(p => value.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}
