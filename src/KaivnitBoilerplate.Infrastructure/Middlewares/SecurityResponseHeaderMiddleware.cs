using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KaivnitBoilerplate.Infrastructure.Middlewares;

public class SecurityResponseHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityResponseHeaderMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public SecurityResponseHeaderMiddleware(
        RequestDelegate next,
        ILogger<SecurityResponseHeaderMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // SECURITY: Remove information disclosure headers
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        context.Response.Headers.Remove("X-AspNet-Version");
        context.Response.Headers.Remove("X-AspNetMvc-Version");

        // SECURITY: X-Content-Type-Options (prevent MIME sniffing)
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // SECURITY: X-Frame-Options (prevent clickjacking)
        var xFrameOptions = _configuration["Security:XFrameOptions"] ?? "DENY";
        context.Response.Headers.Append("X-Frame-Options", xFrameOptions);

        // SECURITY: X-XSS-Protection (legacy XSS filter)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // SECURITY: Referrer-Policy (control referrer information)
        var referrerPolicy = _configuration["Security:ReferrerPolicy"] ?? "strict-origin-when-cross-origin";
        context.Response.Headers.Append("Referrer-Policy", referrerPolicy);

        // SECURITY: Strict-Transport-Security (HSTS)
        var hstsMaxAge = _configuration["Security:HstsMaxAgeSeconds"] ?? "31536000";
        var includeSubDomains = _configuration.GetValue<bool>("Security:HstsIncludeSubDomains", true);
        var preload = _configuration.GetValue<bool>("Security:HstsPreload", true);

        var hstsValue = $"max-age={hstsMaxAge}";
        if (includeSubDomains) hstsValue += "; includeSubDomains";
        if (preload) hstsValue += "; preload";

        context.Response.Headers.Append("Strict-Transport-Security", hstsValue);

        // SECURITY: Permissions-Policy (restrict browser features)
        var permissionsPolicy = _configuration["Security:PermissionsPolicy"]
            ?? "geolocation=(), microphone=(), camera=(), payment=(), usb=()";
        context.Response.Headers.Append("Permissions-Policy", permissionsPolicy);

        // SECURITY: X-Permitted-Cross-Domain-Policies
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");

        // SECURITY: Cross-Origin-Embedder-Policy
        var coep = _configuration["Security:CrossOriginEmbedderPolicy"] ?? "require-corp";
        context.Response.Headers.Append("Cross-Origin-Embedder-Policy", coep);

        // SECURITY: Cross-Origin-Opener-Policy
        var coop = _configuration["Security:CrossOriginOpenerPolicy"] ?? "same-origin";
        context.Response.Headers.Append("Cross-Origin-Opener-Policy", coop);

        // SECURITY: Cross-Origin-Resource-Policy
        var corp = _configuration["Security:CrossOriginResourcePolicy"] ?? "same-origin";
        context.Response.Headers.Append("Cross-Origin-Resource-Policy", corp);

        // SECURITY: Expect-CT (Certificate Transparency)
        context.Response.Headers.Append("Expect-CT", "max-age=86400");

        // SECURITY: NEL (Network Error Logging)
        var nelEnabled = _configuration.GetValue<bool>("Security:EnableNel", true);
        if (nelEnabled)
        {
            var nelReportTo = _configuration["Security:NelReportTo"] ?? "default";
            var nelMaxAge = _configuration["Security:NelMaxAgeSeconds"] ?? "86400";
            context.Response.Headers.Append("NEL",
                $"{{\"report_to\":\"{nelReportTo}\",\"max_age\":{nelMaxAge},\"include_subdomains\":true}}");
        }

        // SECURITY: Report-To API
        var reportToEnabled = _configuration.GetValue<bool>("Security:EnableReportTo", true);
        if (reportToEnabled)
        {
            var reportToUrl = _configuration["Security:ReportToUrl"] ?? "/api/csp-report";
            context.Response.Headers.Append("Report-To",
                $"{{\"group\":\"default\",\"max_age\":31536000,\"endpoints\":[{{\"url\":\"{reportToUrl}\"}}],\"include_subdomains\":true}}");
        }

        // SECURITY: Reporting-Endpoints
        var reportingEndpointsEnabled = _configuration.GetValue<bool>("Security:EnableReportingEndpoints", true);
        if (reportingEndpointsEnabled)
        {
            var reportingUrl = _configuration["Security:ReportToUrl"] ?? "/api/csp-report";
            context.Response.Headers.Append("Reporting-Endpoints", $"default=\"{reportingUrl}\"");
        }

        // SECURITY: Origin-Agent-Cluster
        var originAgentCluster = _configuration.GetValue<bool>("Security:EnableOriginAgentCluster", true);
        if (originAgentCluster)
        {
            context.Response.Headers.Append("Origin-Agent-Cluster", "?1");
        }

        // SECURITY: X-DNS-Prefetch-Control
        var dnsPrefetchControl = _configuration["Security:DnsPrefetchControl"] ?? "off";
        context.Response.Headers.Append("X-DNS-Prefetch-Control", dnsPrefetchControl);

        // SECURITY: Timing-Allow-Origin
        var timingAllowOrigin = _configuration["Security:TimingAllowOrigin"];
        if (!string.IsNullOrEmpty(timingAllowOrigin))
        {
            context.Response.Headers.Append("Timing-Allow-Origin", timingAllowOrigin);
        }

        // SECURITY: X-Download-Options
        context.Response.Headers.Append("X-Download-Options", "noopen");

        // SECURITY: Accept-CH (Client Hints)
        var acceptCh = _configuration["Security:AcceptClientHints"];
        if (!string.IsNullOrEmpty(acceptCh))
        {
            context.Response.Headers.Append("Accept-CH", acceptCh);
        }

        // SECURITY: Critical-CH
        var criticalCh = _configuration["Security:CriticalClientHints"];
        if (!string.IsNullOrEmpty(criticalCh))
        {
            context.Response.Headers.Append("Critical-CH", criticalCh);
        }

        // SECURITY: Vary header
        var varyHeaders = _configuration["Security:VaryHeaders"]
            ?? "Accept, Accept-Encoding, Accept-Language, User-Agent, Authorization";
        if (!context.Response.Headers.ContainsKey("Vary"))
        {
            context.Response.Headers.Append("Vary", varyHeaders);
        }

        // SECURITY: X-Robots-Tag
        var robotsTag = _configuration["Security:XRobotsTag"] ?? "noindex, nofollow, noarchive, nosnippet";
        context.Response.Headers.Append("X-Robots-Tag", robotsTag);

        // SECURITY: Document-Policy
        var documentPolicy = _configuration["Security:DocumentPolicy"];
        if (!string.IsNullOrEmpty(documentPolicy))
        {
            context.Response.Headers.Append("Document-Policy", documentPolicy);
        }

        // SECURITY: Upgrade-Insecure-Requests
        var upgradeInsecure = _configuration.GetValue<bool>("Security:UpgradeInsecureRequests", true);
        if (upgradeInsecure)
        {
            context.Response.Headers.Append("Upgrade-Insecure-Requests", "1");
        }

        await _next(context);
    }
}
