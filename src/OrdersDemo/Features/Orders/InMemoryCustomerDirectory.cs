namespace OrdersDemo.Features.Orders;

/// <summary>In-memory stand-in for a database or remote directory. The delay keeps the async path observable.</summary>
internal sealed class InMemoryCustomerDirectory : ICustomerDirectory
{
    private static readonly TimeSpan SimulatedLatency = TimeSpan.FromMilliseconds(20);

    private static readonly HashSet<string> Customers = new(StringComparer.OrdinalIgnoreCase)
    {
        "ada@example.com",
        "grace@example.com",
    };

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken)
    {
        await Task.Delay(SimulatedLatency, cancellationToken);
        return Customers.Contains(email);
    }
}
