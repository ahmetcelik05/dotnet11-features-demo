namespace OrdersDemo.Features.Reconciliation;

public enum ReconciliationStatus
{
    Ok,
    QuantityMismatch,
    MissingInErp,
    MissingInWarehouse,
}
