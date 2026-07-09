namespace Pharmacy.Data.Entities;

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Units { get; set; }
}