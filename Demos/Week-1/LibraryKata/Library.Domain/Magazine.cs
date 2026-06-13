namespace LibraryKata.Domain;

public class Magazine : LibraryItem, Ilendable
{
    public int CirculationCopies { get; private set; }
    public string Publisher { get; private set; }

    public Magazine(string title, string author, int circulationCopies, string publisher) : base(title, author)
    {
        CirculationCopies = circulationCopies;
        Publisher = publisher;
    }

    public override string Describe()
    {
        return $"{Title} magazine, publish by {Publisher}";
    }

    //Calling this methid in an object insttiated like this:
    // LibraryItem sportsIlustrated = new Magazine() - call libraryItem's Shelfabel
    //This is most likely not what you want
    //Check how new vs override keyword has a different behavior
    public new string Shelfabel()
    {
        return $"MAG--{Id} {Title}";
    }

    // Methods below pasted fom OldBook.cs
    // an acces modifier + return type + method name + parameters (if any)
    public bool Checkout()
    {
        if (CirculationCopies == 0)
        {
            return false;
        }
        //Otherwise, we pass over the above code block
        //We can decrement the available copies and return true
        CirculationCopies--;
        return true;
    }

    public void Return() => CirculationCopies++;
}