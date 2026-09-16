namespace OrdersDemo.Features.Reconciliation;

/// <summary>A system that reports stock levels, such as the warehouse or the ERP.</summary>
public interface IStockSource<TStock>
{
    Task<IReadOnlyList<TStock>> GetStockAsync(CancellationToken cancellationToken);
}
