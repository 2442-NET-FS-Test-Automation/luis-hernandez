using System.ComponentModel.DataAnnotations;

namespace Pharmacy.Data.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;

    // A client can have multiple orders
    public List<Order> Orders { get; set; } = new();
}