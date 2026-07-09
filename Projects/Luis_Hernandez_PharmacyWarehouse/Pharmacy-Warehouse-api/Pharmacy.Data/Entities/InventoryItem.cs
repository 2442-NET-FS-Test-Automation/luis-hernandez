namespace Pharmacy.Data.Entities;

public class InventoryItem
{
    public int Id { get; set; }

    public int CurrentStock { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    //We will use this to track concurrency
    public byte[] RowVersion { get; set; } = default!;
}