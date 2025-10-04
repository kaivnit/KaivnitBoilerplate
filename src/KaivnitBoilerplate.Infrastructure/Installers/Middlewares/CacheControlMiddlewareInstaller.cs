using KaivnitBoilerplate.Application.Abstractions;
using KaivnitBoilerplate.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace KaivnitBoilerplate.Infrastructure.Installers.Middlewares;

[MiddlewareSkip]
public sealed class CacheControlMiddlewareInstaller : IMiddlewareInstaller
{
    public void InstallMiddleware(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<CacheControlMiddleware>();
    }
}
