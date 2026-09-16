namespace OrdersDemo.Features.Orders;

public interface IInventoryService
{
    Task<int> GetAvailableQuantityAsync(string sku, CancellationToken cancellationToken);
}
