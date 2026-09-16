using Microsoft.Extensions.Diagnostics.Tracing;

namespace OrdersDemo.Infrastructure.Tracing;

public static class TracingServiceCollectionExtensions
{
    /// <summary>Configuration section that holds the tracing rules (see appsettings.json).</summary>
    public const string ConfigurationSectionName = "Tracing";

    public static IServiceCollection AddDemoTracing(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTracing(tracing =>
        {
            tracing.AddListener(ConsoleActivityListener.Name, ConsoleActivityListener.Configure);

            // Rules come from configuration; with reloadOnChange they can be flipped without a restart.
            tracing.AddConfiguration(configuration.GetSection(ConfigurationSectionName));
        });

        return services;
    }
}
