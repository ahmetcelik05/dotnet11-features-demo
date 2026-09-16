namespace OrdersDemo.Features.Orders;

/// <summary>In-memory stand-in for an inventory system. The delay keeps the async path observable.</summary>
internal sealed class InMemoryInventoryService : IInventoryService
{
    private static readonly TimeSpan SimulatedLatency = TimeSpan.FromMilliseconds(20);

    private static readonly Dictionary<string, int> Stock = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LAPTOP-15"] = 5,
        ["MOUSE-01"] = 0,
        ["DESK-STD"] = 2,
    };

    public async Task<int> GetAvailableQuantityAsync(string sku, CancellationToken cancellationToken)
    {
        await Task.Delay(SimulatedLatency, cancellationToken);
        return Stock.GetValueOrDefault(sku);
    }
}
