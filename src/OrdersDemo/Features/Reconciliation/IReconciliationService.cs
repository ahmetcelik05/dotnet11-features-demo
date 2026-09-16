namespace OrdersDemo.Features.Reconciliation;

public interface IReconciliationService
{
    IReadOnlyList<ReconciliationRow> Reconcile(IEnumerable<WarehouseStock> warehouse, IEnumerable<ErpStock> erp);
}
