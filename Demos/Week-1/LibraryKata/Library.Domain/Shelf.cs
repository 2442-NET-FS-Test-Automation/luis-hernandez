//For demo sake, lets write a generic

//I want to create a shelf, and a shelf can hold anything
//I can put like computer hardware or supplies, on the shelf
namespace LibraryKata.Domain;

// T is the standard placeholder for... "some type" thet we will apecify later
public class Shelf<T>
{
    private readonly T[] _slots;
    private int used;

    public Shelf(int capacity)
    {
        _slots = new T[capacity];
    }

    public int Capacity => _slots.Length;

    public int Count => used;

    // Method to add items to out shelf
    public bool TryAdd(T item)
    {
        if (used == Capacity)
        {
            return false;
        }

        // If the self isn't full then
        _slots[used++] = item;
        return true;
    }

    //Method to allow index access
    public T Get(int index)
    {
        return _slots[index];
    }
}
