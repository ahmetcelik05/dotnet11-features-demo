using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Tracing;
using Microsoft.Extensions.Hosting;
using OrdersDemo.Features.Orders;

namespace OrdersDemo.Tests.Tracing;

/// <summary>Builds a generic host with a recording listener so tests can observe which activities the rules let through.</summary>
internal static class TracingTestHost
{
    public const string ListenerName = "test";

    public static async Task<IHost> StartAsync(
        Action<ITracingBuilder> configureRules,
        List<string> stoppedOperations,
        bool startHost = true,
        CancellationToken cancellationToken = default)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddTracing(tracing =>
        {
            tracing.AddListener(ListenerName, listener =>
            {
                listener.Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded;
                listener.ActivityStopped = activity => stoppedOperations.Add(activity.OperationName);
            });
            configureRules(tracing);
        });
        builder.Services.AddSingleton<IOrderService, OrderService>();

        var host = builder.Build();
        if (startHost)
        {
            await host.StartAsync(cancellationToken); // listeners are activated when the host starts
        }

        return host;
    }
}
