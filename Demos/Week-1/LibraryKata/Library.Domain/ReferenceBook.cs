using LibraryKata.Domain;

namespace Library.Domain;

public class ReferenceBook : LibraryItem
{
    public string Section { get; }

    public ReferenceBook(string title, string author, string section) : base(title, author)
    {
        Section = section;
    }

    public override string Describe()
    {
        return $"{Id}: {Title} by {Author} -- reference only, {Section} section";
    }

    // Override Shelfabel() - this is a "true" override
    public override string Shelfabel()
    {
        return $"REF--{Id} {Title} {Section}";
    } 
}