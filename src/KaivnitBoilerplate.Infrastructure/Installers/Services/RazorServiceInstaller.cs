using KaivnitBoilerplate.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KaivnitBoilerplate.Infrastructure.Installers.Services;

public sealed class RazorServiceInstaller : IServiceInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRazorPages();
    }
}
