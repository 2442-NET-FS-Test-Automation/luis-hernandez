namespace Pharmacy.Data.Entities;

public class Dispatcher
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    public DispatcherStatus Status { get; set; }

    //We will use this to track concurrency
    public byte[] RowVersion { get; set; } = default!;
}