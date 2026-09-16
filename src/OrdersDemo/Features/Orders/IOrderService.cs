namespace OrdersDemo.Features.Orders;

public interface IOrderService
{
    Task<string> PlaceOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);

    Task<bool> CheckHealthAsync(CancellationToken cancellationToken);
}
