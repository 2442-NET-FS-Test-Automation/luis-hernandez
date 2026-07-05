using Microsoft.EntityFrameworkCore;

namespace Pharmacy.Data.Entities;

public class Order
{
    public int Id { get; set; }

    [Precision(10,2)]
    public decimal TotalPrice { get; set; }

    public OrderPriority Priority { get; set; }
    public OrderStatus Status { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public int DispatcherId { get; set; }
    public Dispatcher Dispatcher { get; set; } = default!;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedUtc { get; set; }

    //Every order has one or more OrderLines (actual product and quantity for this order)
    public List<OrderLine> Lines { get; set; } = new();
}