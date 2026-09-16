using System.ComponentModel.DataAnnotations;

namespace OrdersDemo.Features.Orders;

public sealed class OrderLine
{
    [Required]
    public string Sku { get; init; } = string.Empty;

    [Range(1, 100)]
    public int Quantity { get; init; }
}
