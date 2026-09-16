using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Tracing;
using OrdersDemo.Features.Orders;
using OrdersDemo.Infrastructure.Tracing;

namespace OrdersDemo.Tests.Tracing;

public sealed class TracingRulesTests
{
    private static readonly CreateOrderRequest SampleOrder = new()
    {
        CustomerEmail = "ada@example.com",
        Lines = [new OrderLine { Sku = "LAPTOP-15", Quantity = 1 }],
    };

    [Fact]
    public async Task Rules_enable_a_source_and_disable_one_operation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var stopped = new List<string>();
        using var host = await TracingTestHost.StartAsync(
            tracing => tracing
                .EnableTracing(sourceName: TracingSources.Orders)
                .DisableTracing(sourceName: TracingSources.Orders, operationName: OrderOperations.HealthCheck),
            stopped,
            cancellationToken: cancellationToken);
        var orders = host.Services.GetRequiredService<IOrderService>();

        await orders.PlaceOrderAsync(SampleOrder, cancellationToken);
        await orders.CheckHealthAsync(cancellationToken);

        Assert.Equal([OrderOperations.PlaceOrder], stopped);
    }

    [Fact]
    public async Task Without_rules_nothing_is_traced()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var stopped = new List<string>();
        using var host = await TracingTestHost.StartAsync(_ => { }, stopped, cancellationToken: cancellationToken);
        var orders = host.Services.GetRequiredService<IOrderService>();

        await orders.PlaceOrderAsync(SampleOrder, cancellationToken);

        Assert.Empty(stopped);
    }

    [Fact]
    public async Task Rules_from_configuration_match_the_code_based_rules()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:EnabledTracing:OrdersDemo.Orders:Default"] = "true",
            ["Tracing:EnabledTracing:OrdersDemo.Orders:HealthCheck"] = "false",
        }).Build();

        var stopped = new List<string>();
        using var host = await TracingTestHost.StartAsync(
            tracing => tracing.AddConfiguration(configuration.GetSection(TracingServiceCollectionExtensions.ConfigurationSectionName)),
            stopped,
            cancellationToken: cancellationToken);
        var orders = host.Services.GetRequiredService<IOrderService>();

        await orders.PlaceOrderAsync(SampleOrder, cancellationToken);
        await orders.CheckHealthAsync(cancellationToken);

        Assert.Equal([OrderOperations.PlaceOrder], stopped);
    }

    [Fact]
    public async Task A_bare_boolean_in_configuration_is_shorthand_for_the_source_default()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Tracing:EnabledTracing:OrdersDemo.*"] = "true",
        }).Build();

        var stopped = new List<string>();
        using var host = await TracingTestHost.StartAsync(
            tracing => tracing.AddConfiguration(configuration.GetSection(TracingServiceCollectionExtensions.ConfigurationSectionName)),
            stopped,
            cancellationToken: cancellationToken);
        var orders = host.Services.GetRequiredService<IOrderService>();

        await orders.PlaceOrderAsync(SampleOrder, cancellationToken);
        await orders.CheckHealthAsync(cancellationToken);

        Assert.Equal([OrderOperations.PlaceOrder, OrderOperations.HealthCheck], stopped);
    }

    [Theory]
    [InlineData(false, true, true)]  // disable, then enable  -> enabled
    [InlineData(true, false, false)] // enable, then disable  -> disabled
    public async Task Among_equally_specific_rules_the_last_registered_wins(bool first, bool second, bool expectTraced)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var stopped = new List<string>();
        using var host = await TracingTestHost.StartAsync(
            tracing =>
            {
                Apply(tracing, first);
                Apply(tracing, second);
            },
            stopped,
            cancellationToken: cancellationToken);
        var orders = host.Services.GetRequiredService<IOrderService>();

        await orders.PlaceOrderAsync(SampleOrder, cancellationToken);

        Assert.Equal(expectTraced ? [OrderOperations.PlaceOrder] : [], stopped);

        static void Apply(ITracingBuilder tracing, bool enable)
        {
            if (enable)
            {
                tracing.EnableTracing(sourceName: TracingSources.Orders);
            }
            else
            {
                tracing.DisableTracing(sourceName: TracingSources.Orders);
            }
        }
    }

    [Fact]
    public async Task Changing_configuration_at_runtime_refreshes_the_listener()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var configurationFile = new TemporaryTracingConfiguration(healthCheckEnabled: false);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configurationFile.Path, optional: false, reloadOnChange: true)
            .Build();

        var stopped = new List<string>();
        using var host = await TracingTestHost.StartAsync(
            tracing => tracing.AddConfiguration(configuration.GetSection(TracingServiceCollectionExtensions.ConfigurationSectionName)),
            stopped,
            cancellationToken: cancellationToken);
        var orders = host.Services.GetRequiredService<IOrderService>();

        await orders.CheckHealthAsync(cancellationToken);
        Assert.Empty(stopped);

        configurationFile.Write(healthCheckEnabled: true);
        await WaitUntilAsync(
            async () =>
            {
                await orders.CheckHealthAsync(cancellationToken);
                return stopped.Contains(OrderOperations.HealthCheck);
            },
            cancellationToken);

        Assert.Contains(OrderOperations.HealthCheck, stopped);
    }

    [Theory]
    [InlineData("Legacy.Billing")]
    [InlineData("Legacy.*")]
    [InlineData("Legacy")]
    public async Task Rules_also_apply_to_sources_created_with_the_constructor(string sourceNamePattern)
    {
        var stopped = new List<string>();
        using var host = await TracingTestHost.StartAsync(
            tracing => tracing.EnableTracing(sourceName: sourceNamePattern),
            stopped,
            cancellationToken: TestContext.Current.CancellationToken);
        using var legacySource = new ActivitySource("Legacy.Billing"); // not created through ActivitySourceFactory

        using (legacySource.StartActivity("Charge"))
        {
        }

        Assert.Equal(["Charge"], stopped);
    }

    [Fact]
    public async Task Listeners_are_inactive_until_the_host_starts_or_the_factory_is_resolved()
    {
        var stopped = new List<string>();
        using var host = await TracingTestHost.StartAsync(
            tracing => tracing.EnableTracing(sourceName: "Legacy.*"),
            stopped,
            startHost: false,
            cancellationToken: TestContext.Current.CancellationToken);
        using var legacySource = new ActivitySource("Legacy.Billing");

        using (legacySource.StartActivity("BeforeStart"))
        {
        }

        Assert.Empty(stopped); // nothing listens yet

        host.Services.GetRequiredService<ActivitySourceFactory>(); // same effect as host.StartAsync()
        using (legacySource.StartActivity("AfterFactory"))
        {
        }

        Assert.Equal(["AfterFactory"], stopped);
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition, CancellationToken cancellationToken)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);

        while (!linked.IsCancellationRequested)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(200), linked.Token);
        }

        Assert.Fail("Timed out waiting for the tracing rules to refresh.");
    }
}
