using System.ComponentModel.DataAnnotations;

namespace Pharmacy.Data.Entities;

public class Laboratory
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;

    //Every Laboratory produces one or more Medicines
    public List<Product> Medicines { get; set; } = new();
}