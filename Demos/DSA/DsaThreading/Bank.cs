namespace DsaThreading;

class Bank
{
    public long Balance;

    private readonly object _gate = new();

    public void DepositUnsafe(long amount) => Balance += amount; //Read-modify-write: NOT ATOMIC

    public void DepositSage(long amount)
    {
        lock (_gate) //Only one thread can access per time
        {
            Balance += amount;
        }
    }
}