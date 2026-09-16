using System.Diagnostics;

namespace OrdersDemo.Features.Reconciliation;

/// <summary>
/// Demonstrates the .NET 11 <c>FullJoin</c> operator: every SKU from both systems appears exactly once,
/// whether it exists on one side or both.
/// </summary>
internal sealed class ReconciliationService : IReconciliationService
{
    public IReadOnlyList<ReconciliationRow> Reconcile(IEnumerable<WarehouseStock> warehouse, IEnumerable<ErpStock> erp)
    {
        // Tuple-returning overload: no result selector, optional key comparer.
        var pairs = warehouse.FullJoin(
            erp,
            w => w.Sku,
            e => e.Sku,
            StringComparer.OrdinalIgnoreCase);

        return [.. pairs.Select(ToRow)];
    }

    private static ReconciliationRow ToRow((WarehouseStock? Warehouse, ErpStock? Erp) pair) => pair switch
    {
        (null, { } erp) => new ReconciliationRow(erp.Sku, ReconciliationStatus.MissingInWarehouse, null, erp.Quantity),
        ({ } warehouse, null) => new ReconciliationRow(warehouse.Sku, ReconciliationStatus.MissingInErp, warehouse.Quantity, null),
        ({ } warehouse, { } erp) when warehouse.Quantity != erp.Quantity
            => new ReconciliationRow(warehouse.Sku, ReconciliationStatus.QuantityMismatch, warehouse.Quantity, erp.Quantity),
        ({ } warehouse, { } erp) => new ReconciliationRow(warehouse.Sku, ReconciliationStatus.Ok, warehouse.Quantity, erp.Quantity),
        (null, null) => throw new UnreachableException("FullJoin never yields an empty pair."),
    };
}
