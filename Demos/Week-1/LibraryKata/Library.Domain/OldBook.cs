namespace Library.Domain;

public class OldBook
{
    //Because I want to use a no-arg Constructor, its best practice to make my properties nulable
    public string? Title { get; private set; } // auto property syntax - compiler will generate a private field for us
    public string? Author { get; private set; }
    public int? CopiesAvailable { get; private set; }

    //We can have static properties/members
    private static int nextId = 1; //By convention, static properties have underscore

    public int Id { get; } //no setter, because I dont want somenone to reassign it
    
    //Every class has a very specific method within it The constructor
    //you can have as many other methods as you need/want
    public OldBook(string title, string author, int copiesAvailable)
    {
        Id = nextId++;
        Title = title;
        Author = author;
        CopiesAvailable = copiesAvailable;


    }

    public OldBook() { }

    // an acces modifier + return type + method name + parameters (if any)
    public bool Checkout()
    {
        if (CopiesAvailable == 0)
        {
            return false;
        }
        //Otherwise, we pass over the above code block
        //We can decrement the available copies and return true
        CopiesAvailable--;
        return true;
    }

    public void Return() => CopiesAvailable++;

    public override string ToString()
    {
        //A call to base.String() is a call to refer to a parent class's implementation of a method.
        //Book parent is object, so this is a calling to the default implementation of ToString()
        //return base.ToString();

        return $"Title: {Title}, Author: {Author}, Copies Available: {CopiesAvailable}";
    }

}
