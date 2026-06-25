namespace LibraryKata.Domain;

//An excption is any class tht inherits from the base xception class
public class LibraryException : Exception
{
    //The base class just contains a message
    public LibraryException(string message) : base(message){ }
}