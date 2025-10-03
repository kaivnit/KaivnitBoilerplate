using KaivnitBoilerplate.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace KaivnitBoilerplate.Infrastructure.Installers.Middlewares;

[MiddlewareOrder(5)]
public sealed class RoutingMiddlewareInstaller : IMiddlewareInstaller
{
    public void InstallMiddleware(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
    }
}
