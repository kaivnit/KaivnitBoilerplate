using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace KaivnitBoilerplate.Application.Abstractions;

public interface IMiddlewareInstaller
{
    void InstallMiddleware(IApplicationBuilder app, IWebHostEnvironment env);
}
