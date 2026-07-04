namespace Library.Data.Entities;

public class FulfillmentEvent
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    // = default! is something we're doing for EF Core. I don't want the database column to allow a null
    // =default! ñet e shove dome default value (varies per type) into the properties of creation
    public string Type { get; set; } = default!;
    public DateTime FulfilledAtUtc { get; set; } = DateTime.UtcNow;

}