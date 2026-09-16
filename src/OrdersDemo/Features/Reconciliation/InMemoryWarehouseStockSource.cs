namespace OrdersDemo.Features.Reconciliation;

internal sealed class InMemoryWarehouseStockSource : IStockSource<WarehouseStock>
{
    private static readonly IReadOnlyList<WarehouseStock> Stock =
    [
        new("LAPTOP-15", 5),
        new("mouse-01", 0), // different casing on purpose: the join uses a case-insensitive comparer
        new("CHAIR-ERG", 7),
    ];

    public Task<IReadOnlyList<WarehouseStock>> GetStockAsync(CancellationToken cancellationToken) => Task.FromResult(Stock);
}
