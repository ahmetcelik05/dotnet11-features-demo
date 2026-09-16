using System.Diagnostics;
using OrdersDemo.Infrastructure.Tracing;

namespace OrdersDemo.Features.Orders;

/// <summary>
/// Produces activities from an <see cref="ActivitySource"/> created through the .NET 11
/// <see cref="ActivitySourceFactory"/>. Which activities are recorded is decided by tracing rules, not here.
/// </summary>
internal sealed class OrderService : IOrderService
{
    private static readonly TimeSpan SimulatedProcessingTime = TimeSpan.FromMilliseconds(10);

    private readonly ActivitySource _activitySource;

    public OrderService(ActivitySourceFactory activitySourceFactory)
    {
        _activitySource = activitySourceFactory.Create(new ActivitySourceOptions(TracingSources.Orders));
    }

    public async Task<string> PlaceOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity(OrderOperations.PlaceOrder);
        activity?.SetTag("order.customer", request.CustomerEmail);
        activity?.SetTag("order.lines", request.Lines.Count);

        await Task.Delay(SimulatedProcessingTime, cancellationToken);

        var orderId = $"ORD-{Random.Shared.Next(1000, 9999)}";
        activity?.SetTag("order.id", orderId);
        return orderId;
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken)
    {
        // Frequent and low value: a natural candidate for "disable by rule".
        using var activity = _activitySource.StartActivity(OrderOperations.HealthCheck);
        await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);
        return true;
    }
}
