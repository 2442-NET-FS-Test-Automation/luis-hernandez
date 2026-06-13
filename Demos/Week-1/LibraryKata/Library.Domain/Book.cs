namespace LibraryKata.Domain;

public class Book : LibraryItem, Ilendable
{
    // We include what is unique to a book
    public int CopiesAvailable { get; private set; }

    //Child class constructor look a little different
    public Book(string title, string author, int copiesAvailable) : base(title, author)
    {
        CopiesAvailable = copiesAvailable;
    }

    //Because we have an abstract methid in the parent, we MUST override it or we can't compile
    public override string Describe()
    {
        return $"{Id}: {Title} by  {Author} has {CopiesAvailable} copies available for checkout";
    }

    // Methods below pasted fom OldBook.cs
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
}