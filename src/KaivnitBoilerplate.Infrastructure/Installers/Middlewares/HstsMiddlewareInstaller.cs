using KaivnitBoilerplate.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace KaivnitBoilerplate.Infrastructure.Installers.Middlewares;

[MiddlewareOrder(2)]
public sealed class HstsMiddlewareInstaller : IMiddlewareInstaller
{
    public void InstallMiddleware(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
        {
            app.UseHsts();
        }
    }
}
