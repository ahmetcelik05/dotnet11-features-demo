namespace OrdersDemo.Features.Orders;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Minimal API validation (AddValidation) runs the async validators before this handler executes.
        endpoints.MapPost("/orders", async (CreateOrderRequest request, IOrderService orders, CancellationToken cancellationToken) =>
        {
            var orderId = await orders.PlaceOrderAsync(request, cancellationToken);
            return Results.Created($"/orders/{orderId}", new OrderCreatedResponse(orderId));
        });

        endpoints.MapGet("/health", async (IOrderService orders, CancellationToken cancellationToken) =>
            Results.Ok(new HealthResponse(await orders.CheckHealthAsync(cancellationToken))));

        return endpoints;
    }
}
