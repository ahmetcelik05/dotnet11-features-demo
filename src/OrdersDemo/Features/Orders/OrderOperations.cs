namespace OrdersDemo.Features.Orders;

/// <summary>Activity operation names emitted by <see cref="OrderService"/>; referenced by tracing rules.</summary>
public static class OrderOperations
{
    public const string PlaceOrder = "PlaceOrder";
    public const string HealthCheck = "HealthCheck";
}
