namespace OrdersDemo.Features.Reconciliation;

public static class ReconciliationEndpoints
{
    public static IEndpointRouteBuilder MapReconciliationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/reconciliation", async (
            IStockSource<WarehouseStock> warehouse,
            IStockSource<ErpStock> erp,
            IReconciliationService reconciliation,
            CancellationToken cancellationToken) =>
        {
            var warehouseStock = await warehouse.GetStockAsync(cancellationToken);
            var erpStock = await erp.GetStockAsync(cancellationToken);
            return Results.Ok(reconciliation.Reconcile(warehouseStock, erpStock));
        });

        return endpoints;
    }
}
