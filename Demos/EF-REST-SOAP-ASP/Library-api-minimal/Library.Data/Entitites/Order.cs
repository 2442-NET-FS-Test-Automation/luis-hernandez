namespace Library.Data.Entities;

public class Order
{
    public int Id { get; set; }
    //Nice to have access to the foreign key without having to load and pass the entire object.
    public int CustomerId { get; set; } // FK -> Customer 
    public Customer Customer { get; set; } = default!;

    public Priority Priority { get; set; }

    public Status Status { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedUtc { get; set; }

    //Every order has one or more productLine
    // Orderline are the actual product and quantity of a  something on the order
    public List<OrderLine> Lines { get; set; } = new();
}