namespace Pharmacy.Data.Exceptions;

public class UnknownBatchException : DomainValidationException
{
    public string Batch { get; }
    public UnknownBatchException(string batch) : base($"Unknown batch: {batch}")
    {
        Batch = batch;
    }
}
