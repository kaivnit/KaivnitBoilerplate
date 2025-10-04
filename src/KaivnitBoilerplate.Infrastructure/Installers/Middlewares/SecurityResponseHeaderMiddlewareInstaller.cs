using KaivnitBoilerplate.Application.Abstractions;
using KaivnitBoilerplate.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace KaivnitBoilerplate.Infrastructure.Installers.Middlewares;

[MiddlewareOrder(10)]
public sealed class SecurityResponseHeaderMiddlewareInstaller : IMiddlewareInstaller
{
    public void InstallMiddleware(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<SecurityResponseHeaderMiddleware>();
    }
}
