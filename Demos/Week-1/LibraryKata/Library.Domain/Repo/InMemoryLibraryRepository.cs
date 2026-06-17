// This class will be our actual library Catalog store of info

using Serilog; //Bringing it that outside code we downloaded
namespace LibraryKata.Domain;

public class InMemoryLibraryRepository : ILibraryRepository
{
    // Because we don't have an outside store of info (like  SQL database)
    //we are kind of forced to rely on a list. We will store info outside
    //of program execution - Promised
    private readonly List<LibraryItem> _items = new();

    public void Add(LibraryItem item)
    {
        _items.Add(item);
        //We just added a new item - thats a significant event. Lets go
        //Using Serilog's template string format
        Log.Information("Adde {Title} - id: {id}", item.Title, item.Id);
    }

    public List<LibraryItem> GetAll()
    {
        //Don't want to accidentally pass a pinter to my real list
        //return a new copy of the list
        return _items.ToList();
    }

    public LibraryItem GetById(int id)
    {
        //Loop thorough the list, check for an item with the given id
        //if we don't find it, thwor that exeption
        foreach (LibraryItem item in _items)
        {
            if (item.Id == id)
            {
                return item;
            }
        }

        Log.Warning("Lookup failed for id {Id}", id);
        throw new ItemNotFoundException(id);
    }

    public bool Remove(int id)
    {
        foreach (LibraryItem item in _items)
        {
            if (item.Id == id)
            {
                _items.Remove(item);
                Log.Information("Removed item with id {Id}", id);
                return true;
            }
        }

        Log.Information("Removal failed for item with id {id}", id);
        return false;
    }
}