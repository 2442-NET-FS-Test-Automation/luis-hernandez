namespace Pharmacy.Data.Entities;

public class Product
{
    public int Id { get; set; }
    public string CommercialName { get; set; } = default!;
    public string PrincipalActive { get; set; } = default!;
    public int Dose { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string Batch { get; set; } = default!;
    public decimal Price { get; set; }

    //A product has an inventory item, an inventory item is as associated with one product
    public int InventoryId { get; set; }
    public InventoryItem InventoryItem { get; set; } = default!;

    public int LaboratoryId { get; set; }
    public Laboratory Laboratory { get; set; } = default!;
}