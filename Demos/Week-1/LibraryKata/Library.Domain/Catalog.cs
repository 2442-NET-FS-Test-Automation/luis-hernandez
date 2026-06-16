namespace LibraryKata.Domain;

public class Catalog
{
    // Backing our catalog is going to ve a list.
    // List<T>: ordered, grow/shrink dynamically, accesible via index
    public readonly List<LibraryItem> Items = new();

    // This method just wraps the above list
    //But we could not only wrap the list, but also add some logic to it
    //For example restrcit people from Adding or Removing or even accesing via index
    //By wrraping its instance methods with out methods and make them internal, private, public, protected, etc
    public int Count => Items.Count;

    //Stack<T> LIFO - We will model a return cart. The most recently returned item is re-shelved first
    //Primary methods: Push, Pop
    public readonly Stack<LibraryItem> ReturnCart = new();

    // Queue<T> FIFO - modeling a hold queue, customers placing hold and books
    //Primary methods: Enqueue, Dequeue
    public readonly Queue<string> HoldQueue = new();

    // Reading List
    //LinkedList<T> - cheap intesert/removals anywhere in my list,  but no index access
    public readonly LinkedList<LibraryItem> ReadingList = new();
}