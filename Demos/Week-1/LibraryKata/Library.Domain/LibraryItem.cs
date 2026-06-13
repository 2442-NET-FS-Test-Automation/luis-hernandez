namespace LibraryKata.Domain;

//Abstract class it cannot be instantiated
// Still have a constructor becasuse child clasess STILL NEED to be able to call their parent's constructor - but WE can't call it via new
public abstract class LibraryItem
{
    //Because I want to use a no-arg Constructor, its best practice to make my properties nulable
    public string? Title { get; private set; } // auto property syntax - compiler will generate a private field for us
    public string? Author { get; private set; }

    //We can have static properties/members
    private static int nextId = 1; //By convention, static properties have underscore

    public int Id { get; } //no setter, because I dont want somenone to reassign it


    //My abstract class DOES have a constructor
    //So far, we've dealt with public and private  access modifiers
    //public: anyone can see/call it
    //private: only accesable within this class
    //protected: this class and derived classes only

    protected LibraryItem(string title, string author)
    {
        Id = nextId++;
        Title = title;
        Author = author;
    }


    //Abstract method - only a signature - no body
    public abstract string Describe();
    //Abstract classes CAN contain concrete implementation
    //Mix of abstract methods and concrete implementation to save time later
    public override string ToString() => Describe();

    //While concrete method have a body, Abstract methods MUST be overriden... 
    //Virtual methods have a body and MAY be overriden
    public virtual string Shelfabel()
    {
        return $"{Id}: {Title}";
    }
}