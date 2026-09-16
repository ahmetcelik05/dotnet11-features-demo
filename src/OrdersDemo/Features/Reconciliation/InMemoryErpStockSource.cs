namespace OrdersDemo.Features.Reconciliation;

internal sealed class InMemoryErpStockSource : IStockSource<ErpStock>
{
    private static readonly IReadOnlyList<ErpStock> Stock =
    [
        new("LAPTOP-15", 5),
        new("MOUSE-01", 3),
        new("DESK-STD", 2),
    ];

    public Task<IReadOnlyList<ErpStock>> GetStockAsync(CancellationToken cancellationToken) => Task.FromResult(Stock);
}
