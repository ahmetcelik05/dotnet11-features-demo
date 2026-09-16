using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OrdersDemo.Features.Products;
using OrdersDemo.Infrastructure.Tracing;

namespace OrdersDemo.Infrastructure.Telemetry;

public static class OpenTelemetryServiceCollectionExtensions
{
    private const int MetricsExportIntervalMilliseconds = 10_000;

    /// <summary>
    /// Exports the cache meter and the orders <c>ActivitySource</c> to the console. OpenTelemetry registers its own
    /// <c>ActivityListener</c>; the tracing rules configured in <see cref="TracingServiceCollectionExtensions"/> do not apply to it.
    /// </summary>
    public static IServiceCollection AddDemoOpenTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddMeter(ProductsServiceCollectionExtensions.CacheMeterName)
                .AddConsoleExporter((_, reader) =>
                    reader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = MetricsExportIntervalMilliseconds))
            .WithTracing(tracing => tracing
                // ASP.NET Core creates an unsampled parent activity for log scopes; the default ParentBased sampler
                // would drop our child spans, which makes it look as if the tracing rules applied to OpenTelemetry.
                .SetSampler(new AlwaysOnSampler())
                .AddSource(TracingSources.Orders)
                .AddConsoleExporter());

        return services;
    }
}
