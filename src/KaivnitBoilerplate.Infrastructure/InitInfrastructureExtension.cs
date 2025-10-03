using KaivnitBoilerplate.Application.Abstractions;
using KaivnitBoilerplate.Infrastructure.Installers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace KaivnitBoilerplate.Infrastructure;

public static class InitInfrastructureExtension
{
    public static void InstallServicesInAssembly<TInstallAssembly, TApplication>(this IServiceCollection services,
        IConfiguration configuration,
        string targetService)
    {
        var installers = typeof(TInstallAssembly).Assembly.ExportedTypes
            .Where(x => typeof(IServiceInstaller).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false })
            .Where(x => !x.GetCustomAttributes(typeof(ServiceSkipAttribute), false).Any()) // Bỏ qua dịch vụ có [ServiceSkip] Attribute
            .Where(x =>
            {
                var targetAttrs = x.GetCustomAttributes<TargetServiceAttribute>();
                if (!targetAttrs.Any())
                {
                    return true; // Nếu không có thuộc tính, luôn cài đặt
                }

                return targetAttrs.Any(attr => attr.ServiceNames.Contains(targetService, StringComparer.OrdinalIgnoreCase));
            })
            .Select(x => new
            {
                Installer = (IServiceInstaller?)Activator.CreateInstance(x),
                Order = x.GetCustomAttributes(typeof(ServiceOrderAttribute), false)
                         .Cast<ServiceOrderAttribute>()
                         .FirstOrDefault()?.Order ?? int.MaxValue
            })
            .Where(x => x.Installer != null)
            .OrderBy(x => x.Order)
            .Select(x => x.Installer!)
            .ToList();

        if (installers.Any())
        {
            installers.ForEach(installer => installer.InstallServices(services, configuration));
        }
    }

    public static void InstallMiddlewaresInAssembly<TInstallAssembly>(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        var installers = typeof(TInstallAssembly).Assembly.ExportedTypes
            .Where(x => typeof(IMiddlewareInstaller).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false })
            .Where(x => !x.GetCustomAttributes(typeof(MiddlewareSkipAttribute), false).Any()) // Bỏ qua middleware có [MiddlewareSkip] Attribute
            .Select(x => new
            {
                Installer = (IMiddlewareInstaller?)Activator.CreateInstance(x),
                Order = x.GetCustomAttributes(typeof(MiddlewareOrderAttribute), false)
                         .Cast<MiddlewareOrderAttribute>()
                         .FirstOrDefault()?.Order ?? int.MaxValue
            })
            .Where(x => x.Installer != null)
            .OrderBy(x => x.Order)
            .Select(x => x.Installer)
            .ToList();

        if (installers.Any())
        {
            installers.ForEach(installer => installer!.InstallMiddleware(app, env));
        }
    }
}
