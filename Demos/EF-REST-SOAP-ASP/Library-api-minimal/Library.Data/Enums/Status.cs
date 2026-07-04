namespace Library.Data.Entities;

public enum Status
{
    // In my application if an order is yet to be processed it is pending.
    // Fulfillment means the sale completed
    // Backordered happens when someone places an order for a product
    Pending,
    Fulfilled,
    Backordered
}