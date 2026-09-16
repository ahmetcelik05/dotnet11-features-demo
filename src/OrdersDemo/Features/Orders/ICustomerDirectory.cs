namespace OrdersDemo.Features.Orders;

public interface ICustomerDirectory
{
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken);
}
