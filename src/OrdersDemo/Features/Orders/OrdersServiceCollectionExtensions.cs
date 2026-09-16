namespace OrdersDemo.Features.Orders;

public static class OrdersServiceCollectionExtensions
{
    public static IServiceCollection AddOrders(this IServiceCollection services)
    {
        services.AddValidation();
        services.AddSingleton<ICustomerDirectory, InMemoryCustomerDirectory>();
        services.AddSingleton<IInventoryService, InMemoryInventoryService>();
        services.AddSingleton<IOrderService, OrderService>();
        return services;
    }
}
