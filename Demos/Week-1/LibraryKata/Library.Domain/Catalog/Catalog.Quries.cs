using System.Collections;
namespace LibraryKata.Domain;

//The second half of my class
//I don't have to mirror the interface implementation or inheritance across both class files
public partial class Catalog : IEnumerable<LibraryItem>
{
    //this is the one that we actually want to provide logic for, the one that uses a generic
    public IEnumerator<LibraryItem> GetEnumerator()
    {
        foreach( LibraryItem item in _items)
        {
            //We want to lazily return item one at a time, we don't want to return a second list
            //or anything like that. We will use "yield" with out return
            yield return item; //Streaming items one at a time
        }
    }

    //This version (non-generic version) is OLD - kept in IEnumerable for backwards compatibility
    //What are we doing is simply routing it to IEnumerator<LibraryItem> GetEnumerator() method
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerable<LibraryItem> lendable()
    {
        foreach (LibraryItem item in _items)
        {
            if (item is Ilendable)
            {
                yield return item;
            }
        }
    }

    //Search function for the catalog
    //We are going to use Predicate to pass a delegate to our function
    // A delegate is just a referencie to a method in an argument list
    //Predicate<LibraryItem> match represent a function that takes a libraryItem, and return a boolean
    //When we call this Find() method, we will combine it with a lambda. lambda's are the C# implementation of anonymus or arrow functions. Just a quick definition that we don't bother storing a reference to.
    // authorizedItems = Find(item=>items.uthor == "Frank Herbert"); - "find every item where it's author equals "Frank Herbert"
    public List<LibraryItem> Find(Predicate<LibraryItem> match)
    {
        //match is a method, not an object or value
        // it is a pointer to some method that gets passed in when we call find()
        List<LibraryItem> foundItems = new();

        foreach (LibraryItem item in _items)
        {
            if (match(item))
            {
                foundItems.Add(item);
            }
        }

        return foundItems;
    }

}