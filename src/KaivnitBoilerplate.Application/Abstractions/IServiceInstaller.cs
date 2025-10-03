using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KaivnitBoilerplate.Application.Abstractions;

public interface IServiceInstaller
{
    void InstallServices(IServiceCollection services, IConfiguration configuration);
}
