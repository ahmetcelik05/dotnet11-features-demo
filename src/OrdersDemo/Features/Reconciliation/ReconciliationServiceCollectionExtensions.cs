using System.Text.Json.Serialization;

namespace OrdersDemo.Features.Reconciliation;

public static class ReconciliationServiceCollectionExtensions
{
    public static IServiceCollection AddReconciliation(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddSingleton<IStockSource<WarehouseStock>, InMemoryWarehouseStockSource>();
        services.AddSingleton<IStockSource<ErpStock>, InMemoryErpStockSource>();
        services.AddSingleton<IReconciliationService, ReconciliationService>();
        return services;
    }
}
