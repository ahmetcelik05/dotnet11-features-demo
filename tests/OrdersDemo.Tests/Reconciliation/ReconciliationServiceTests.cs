using OrdersDemo.Features.Reconciliation;

namespace OrdersDemo.Tests.Reconciliation;

public sealed class ReconciliationServiceTests
{
    private readonly ReconciliationService _service = new();

    [Fact]
    public void Every_sku_from_both_sides_appears_exactly_once()
    {
        WarehouseStock[] warehouse = [new("LAPTOP-15", 5), new("MOUSE-01", 0), new("CHAIR-ERG", 7)];
        ErpStock[] erp = [new("LAPTOP-15", 5), new("MOUSE-01", 3), new("DESK-STD", 2)];

        var rows = _service.Reconcile(warehouse, erp);

        Assert.Equal(4, rows.Count);
        Assert.Equal(["LAPTOP-15", "MOUSE-01", "CHAIR-ERG", "DESK-STD"], rows.Select(r => r.Sku));
        Assert.Equal(
            [
                ReconciliationStatus.Ok,
                ReconciliationStatus.QuantityMismatch,
                ReconciliationStatus.MissingInErp,
                ReconciliationStatus.MissingInWarehouse,
            ],
            rows.Select(r => r.Status));
    }

    [Fact]
    public void Comparer_is_honoured_for_case_insensitive_keys()
    {
        WarehouseStock[] warehouse = [new("chair-erg", 7)];
        ErpStock[] erp = [new("CHAIR-ERG", 7)];

        var rows = _service.Reconcile(warehouse, erp);

        var row = Assert.Single(rows);
        Assert.Equal(ReconciliationStatus.Ok, row.Status);
    }
}
