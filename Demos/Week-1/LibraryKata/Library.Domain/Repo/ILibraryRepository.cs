namespace LibraryKata.Domain;
public interface ILibraryRepository
{
    // This interface is an abstraction over an actual repository class
    //Lets think of thing we want to be able to do against out library's store information

    //At minimum we want to provide for basic CRUD

    //Create new items in my library
    void Add(LibraryItem item); //takes in the item to be added, can be anything that inherits from the parent

    //Read/get library items
    LibraryItem GetById(int id); //throws our ItemNotFoundException exception if the item doesn't exist at all
    List<LibraryItem> GetAll();

    //Update library items

    //Delete items library
    bool Remove(int id); // takes in item id of item to delete
}