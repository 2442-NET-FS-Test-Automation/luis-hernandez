// This class will be our actual library Catalog store of info

using Serilog; //Bringing it that outside code we downloaded
namespace LibraryKata.Domain;

public class InMemoryLibraryRepository : ILibraryRepository
{
    // Because we don't have an outside store of info (like  SQL database)
    //we are kind of forced to rely on a list. We will store info outside
    //of program execution - Promised
    private readonly Dictionary<int, LibraryItem> _items = new();

    public void Add(LibraryItem item)
    {
        _items.Add(item.Id, item);
        //We just added a new item - thats a significant event. Lets log it
        //Using Serilog's template string format
        Log.Information("Adde {Title} - id: {id}", item.Title, item.Id);
    }

    public List<LibraryItem> GetAll()
    {
        //Don't want to accidentally pass a pinter to my real list
        //return a new copy of the list
        return _items.Values.ToList();
    }

    public LibraryItem GetById(int id)
    {
        //OLDbacked list
        //Loop thorough the list, check for an item with the given id
        //if we don't find it, thwor that exeption
        // foreach (LibraryItem item in _items)
        // {
        //     if (item.Id == id)
        //     {
        //         return item;
        //     }
        // }

        //New dictionary backed lookup code
        //TryGetValue uses an out parameter. We pass it some value to do key based lookup
        //We also need to use the out keyword, and give a tyoe and variable name for second return
        if(_items.TryGetValue(id, out LibraryItem? item)) //using an out parameter to get a second return value
        {
            return item;
        } 

        Log.Warning("Lookup failed for id {Id}", id);
        throw new ItemNotFoundException(id);
    }

    public bool Remove(int id)
    {
        // foreach (LibraryItem item in _items)
        // {
        //     if (item.Id == id)
        //     {
        //         _items.Remove(item);
        //         Log.Information("Removed item with id {Id}", id);
        //         return true;
        //     }
        // }

        if (_items.Remove(id)){
            Log.Information("Removed item with id{Id}", id); //log the removal
            return true;
        }

        Log.Information("Removal failed for item with id {id}", id);
        return false;
    }
}