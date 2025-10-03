using KaivnitBoilerplate.Application;
using KaivnitBoilerplate.Infrastructure;
using Microsoft.AspNetCore.Builder;

namespace KaivnitBoilerplate.Presentation;

public class Startup
{
    public IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    /// <summary>
    /// Configures application services and dependencies for the current hosting environment.
    /// </summary>
    /// <param name="services">The collection of service descriptors to which application services are added. This parameter cannot be null.</param>
    /// <param name="env">An object that provides information about the web hosting environment. Used to configure services based on the
    /// current environment.</param>
    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            Console.Title = System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Name ?? "Kaivnit Boilerplate App";

        // Register DI Service
        services.InstallServicesInAssembly<IInfrastructureAssembly, IApplicationAssemblyMarker>(Configuration, "KaivnitBoilerplate");
    }

    /// <summary>
    /// Configures the application's request pipeline.
    /// </summary>
    /// <remarks>Call this method to define how the application responds to HTTP requests by adding middleware
    /// components to the pipeline. The order in which middleware is added is significant and affects request
    /// processing.</remarks>
    /// <param name="app">The application builder used to construct the HTTP request pipeline.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.InstallMiddlewaresInAssembly<IInfrastructureAssembly>(env);
    }
}
