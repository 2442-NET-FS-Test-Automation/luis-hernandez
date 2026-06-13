namespace LibraryKata.Domain;

//Interfaces in C# - they are a contract for behaviors 
public interface Ilendable
{
    //Only method signatures, not bodies, not even acces modifiers
    bool Checkout();
    void Return();
}