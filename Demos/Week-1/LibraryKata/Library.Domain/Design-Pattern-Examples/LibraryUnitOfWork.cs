using Serilog;
namespace LibraryKata.Domain;

public class LibraryUnitOfWork : IUnitOfWork
{
    //Mandatory property member of IUnitInterface
    public ILibraryRepository Items { get; }

    //I want something to hold my list of staged changes
    //we will represent those as strings, this is a shallow demo example
    private readonly List<String> _stages = new();

    //We need a constructor
    //We are technically using dependency injection here, We never instatiate the
    //ILibraryRepository object, we ask for an existing one.
    public LibraryUnitOfWork(ILibraryRepository items)
    {
        Items = items;
    }

    public int Commit()
    {
        //Shallow commit implementation
        //We will just log how many things were staged + commited
        int count = _stages.Count; // how many things are in staging at commit time?

        // Log the count via Serilog
        Log.Information("LibraryUnitOfWork commited {count} staged change(s)", count);

        _stages.Clear();

        return count;

    }

    public void Stage(string change)
    {
        _stages.Add(change); //staging a change
    }
}