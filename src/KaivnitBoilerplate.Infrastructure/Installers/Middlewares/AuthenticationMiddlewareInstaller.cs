using KaivnitBoilerplate.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace KaivnitBoilerplate.Infrastructure.Installers.Middlewares;

[MiddlewareOrder(6)]
public sealed class AuthenticationMiddlewareInstaller : IMiddlewareInstaller
{
    public void InstallMiddleware(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseAuthentication();
    }
}
