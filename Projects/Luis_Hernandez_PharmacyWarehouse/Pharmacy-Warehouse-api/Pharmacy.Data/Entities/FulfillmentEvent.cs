namespace Pharmacy.Data.Entities;

public class FulfillmentEvent
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Type { get; set; } = default!;
    public DateTime FulfilledAtUtc { get; set; } = DateTime.UtcNow;

    // Record wich dispatcher processed this order. 
    // Only here exist the Order<->Dispatcher relation as an historical
    public int DispatcherId { get; set; }
}