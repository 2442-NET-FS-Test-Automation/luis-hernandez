using System.ComponentModel.DataAnnotations;

namespace Pharmacy.Data.Entities;

public class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    // A client can have multiple orders
    public List<Order> Orders { get; set; } = new();
}