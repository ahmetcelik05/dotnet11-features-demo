namespace OrdersDemo.Features.Reconciliation;

public sealed record ReconciliationRow(
    string Sku,
    ReconciliationStatus Status,
    int? WarehouseQuantity,
    int? ErpQuantity);
