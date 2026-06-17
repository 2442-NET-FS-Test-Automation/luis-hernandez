// If I have code from another namespace that I wannt to use her - I use a using satatement to import it.
using System.Data.Common;
using System.Globalization;
using Library.Domain;
using Libraryaa.Domain;
using LibraryKata.Domain;
using Serilog;

namespace LibraryKata.App; // A logical container for different related code files.

public class Program
{

    // public - accessible across the program
    // static - Main can be called upon without a Program object. It is a Static/class method. 
    // void - it doesn't return anything
    public static void Main()
    {
        //Lets configure Serilog here before any code execution
        //Serilog works via a singleton object. Its shared globally
        //throughout the app, configure once use anywhere
        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information() //Verbose > Debug > Info > Warning > Error > Fatal
        .WriteTo.Console() // Sink where do my log go? text file, database, etc?
        .CreateLogger();


        Program.ExceptionDemo();
        Program.AdvancedClassDemo();
        

        Log.CloseAndFlush();
    }

    // private - accessible only within this class
    // static - it belongs to the class, not objects of the class
    // void - returns nothing
    private static void DataTypesAndOperators()
    {
        Console.WriteLine("=== Data types and operators ==");

        // C# is a Strongly typed language
        // Create variables with explicit data types
        int copies = 3; // whole
        double fee = 1.8; // decimals
        double credit = 10;
        bool isMember = true; // true/false values
        char shelf = 'A'; // single character
        string title = "The Little Prince"; // text

        // Operators 
        string user = "Luis"; // Single = is the assignment operator. 
        double total = copies * fee; // arithmetic operator
        bool isEnough = credit >= total; // comparison
                                         //  We want to know if credit is greater than or equal to total
                                         // if credit is greater than or equal to total, isEnough will get 'true', otherwise it will get 'false'

        bool lendable = isMember && isEnough; //logical operators
        // && - and, 
        // || - or, 
        // ! - reverses the condition that follows
        // ^ logical XOR - returns true if ONLY one condition is true

        // String concat
        Console.WriteLine(title + " has been checked out by " + user);

        // String Interpolation
        Console.WriteLine($"{title} on shelf {shelf} has been checked out by {user} with {copies} copies, fee {fee} and total {total}");



    }

    private static void ControlFlow()
    {
        Console.WriteLine("\n=== Control flow ===");

        //If,  else if, else
        int copiesAvailable = 0;
        bool isMember = true;

        if (copiesAvailable > 1)
        {
            Console.WriteLine("Available for checkout");
        }
        else if (copiesAvailable == 1)
        {
            Console.WriteLine("Last copy!");
        }
        else
        {
            Console.WriteLine("Out of stock");
        }

        // Switch statement
        string genre = "Sci-fi";

        //Classic switch
        switch (genre)
        {
            case "Sci-fi":
                Console.WriteLine("Check section A");
                break;
            case "Mistery":
                Console.WriteLine("Check section B");
                break;
            default: //Optional, useful when you want to catch all the cases that are not covered by the previous cases.
                Console.WriteLine("Check the general section");
                break;
        }

        // New switch expression (C# 8.0+)
        // In a switch expression, we can assign the result of the switch to a variable. It is more concise and can be more readable

        string section = genre switch
        {
            "Sci-fi" => "Section A",
            "Mistery" => "Section B",
            _ => "General Section"
        };

        Console.WriteLine($"Check {section}");
    }

    private static void Loops()
    {
        //For, while, do-while, foreach

        for (int day = 1; day <= 3; day++)
        {
            Console.WriteLine($"Day {day}: fee so far is {CalculateFee(day)}");
        }

        int onShelf = 3;
        while (onShelf > 0)
        {
            Console.WriteLine($"There are {onShelf} copies on the shelf");
            onShelf--; //decrement
        }

        Console.WriteLine("No copies on shelf!");

    }

    private static decimal CalculateFee(int daysLate) => daysLate * 2;

    private static void ArraysWork()
    {
        string[] books = { "Dune", "Harry Potter", "Percy Jackson", "Lord od the Rings" };

        Console.WriteLine(books[2]);

        foreach (string book in books)
        {
            Console.WriteLine(book);
        }
    }

    private static void ClassesExample()
    {
        Console.WriteLine("\n=== Using our domain Book class ===");

        //Instatiating my first book, calling the constructor via the new keyword
        Book dune = new Book("Dune", "Frank Herbert", 3);
        Book littlePrince = new Book("The Little Prince", "Antoine de Saint-Exupéry", 2);

        Console.WriteLine(dune);
        Console.WriteLine(littlePrince.ToString());
        //toString is called by default when we try to print an object, 
        // but we can also call it explicitly. 
        // We have overridden the default implementation of ToString() in our Book class

        Console.WriteLine($"Cheching out {dune.Title}: {dune.Checkout()}");
        Console.WriteLine($"Cheching out {littlePrince.Title}: {littlePrince.Checkout()}");
    }

    public static void OopDemo()
    {
        Console.WriteLine("\n\n == OOP Demo stuff ==");

        //Leveraring polymorphism - Books, ReferenceBooks, Magazines - all are LibraryItems
        LibraryItem[] catalog =
        {
            new Book("Dune", "Frank H", 2),
            new ReferenceBook("C# Languge Standards", "Microsoft", "Technology"),
            new Magazine("Sport Ilustrated", "Fransisco", 5, "Conde Naste")
        };

        foreach (LibraryItem item in catalog)
        {
            Console.WriteLine(item.Describe());
        }

        foreach (LibraryItem item in catalog)
        {
            if (item is Ilendable lendable)
            {
                Console.WriteLine($"{item.Title}: checkout -> {lendable.Checkout}");
            }
            else
            {
                Console.WriteLine($"{item.Title} is Reference only");
            }
        }

        //Override vs new
        Magazine wired = new Magazine("Wired", "Luis", 3, "Conde Naste");

        LibraryItem baseMag = wired;

        Console.WriteLine("== Override vs New with the same object ==");

        Console.WriteLine("== Override ==");
        Console.WriteLine($"Magazine Reference -> {wired.Describe()}");
        Console.WriteLine($"LibraryItem Reference -> {baseMag.Describe()}");

        Console.WriteLine("== New ==");
        Console.WriteLine($"Magazine Shelfabel -> {wired.Shelfabel()}");
        Console.WriteLine($"LibraryItem Shelfabel -> {baseMag.Shelfabel()}");
    }

    private static void CollectonsDemo()
    {
        Console.WriteLine("\n\n == Collections Demo ==");

        //Creating a catalog objects
        //Because this is a backed by a list, it grows and shrinks for us
        Catalog catalog = new();

        //I could create my objects
        Book dune = new Book("Dune", "Frank Herbert", 3);

        // Then add them
        catalog.Add(dune);

        catalog.Add(new ReferenceBook("C# Languge Standards", "Microsoft", "Technology"));
        catalog.Add(new Magazine("Sport Ilustrated", "Fransisco", 5, "Conde Naste"));

        Console.WriteLine($"Catalog holds {catalog.Count} items; first is {catalog[0].Title}");

        //Enum + Struct use
        ItemKind kind = ItemKind.Magazine;

        ShelfLocation where = new ShelfLocation(3, 12); // struct - look a lot like a class, but is a VALUE type.

        Console.WriteLine($"The {kind} is located at {where}");

        Book duneCopy = dune; // Because Book is a class, it is a reference type. duneCopy is a reference to the same object as dune

        ShelfLocation where2 = where; // Because ShelfLocation is a struct, it is a value type. where2 is a copy of where, they are two different objects in memory with the same data.

        //Generics: our own Shelf<T> class that can hold anythinf, through tecnically all the collections we used thusfar have been generic classes themselves.
        Shelf<LibraryItem> shelf = new Shelf<LibraryItem>(2);
        Shelf<int> intShelf = new Shelf<int>(200);

        shelf.TryAdd(catalog[0]);
        shelf.TryAdd(catalog[1]);

        Console.WriteLine($"Trying to add a third thing in our catalog: {shelf.TryAdd(catalog[2])}");
    }

    public static void ExceptionDemo()
    {
        Console.WriteLine("\n == Exceptions, patterns, logging ==");

        //By using liskov substitution from SOLID, if I later swap to a SQL Library or whatever, this is the only thing I have to change
        ILibraryRepository repo = new InMemoryLibraryRepository();

        //Injecting our existing repo object to satisfy libraryUnitOfWork's dependency
        IUnitOfWork libraryWork = new LibraryUnitOfWork(repo);

        LibraryItem dune = LibraryItemFactory.Create(ItemKind.Book, "Dune", "Frank Herbert", copies: 3);

        repo.Add(dune);

        repo.Add(LibraryItemFactory.Create(ItemKind.Magazine, "Wired", "Axel", copies: 2));

        //Pretend were commiting changes to a DB or something
        libraryWork.Stage("added 2 items");
        libraryWork.Commit();

        //We went through the trouble of creatung custom exceptions
        //Lets actually see them work for us, If you have code that can potentially fail
        //wrap it in a ry-catch (optinal finally)
        try
        {
            LibraryItem missing = repo.GetById(99);
            Console.WriteLine(missing.Describe());
        }
        catch (ItemNotFoundException ex)
        {
            //Handle exception from most -> specific
            Log.Error("Lookup failes for id {Id}: {Message}", ex.Id, ex.Message);
        }
        catch (LibraryException ex)
        {
            Log.Error("Library error: {}", ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error("Non Library Error: {Message}", ex.Message);
        }
        finally
        {
            //Optional, but adding a finally block adds code that runs whether an exception is caught or not
            Console.WriteLine();
        }

        Book noCopies = new Book("Count of Montecristo", "Alejandro Dumas", 0);

        try
        {
            Borrow(noCopies);
        } 
        catch(ItemNotAvailableException ex)
        {
            Log.Warning("Borrow refused: {Message}", ex.Message);
        }
    }

    public static void Borrow(Book book)
    {
        if (!book.Checkout())
        {
            throw new ItemNotAvailableException(book.Title);
        }
    }
    
    public static void AdvancedClassDemo()
    {
        Console.WriteLine("\n == Advanced classes ==");

        //First, a quick detour, lets interact with GC
        Console.WriteLine(GC.GetTotalMemory(forceFullCollection: false) / 1024);

        ILibraryRepository repo = new InMemoryLibraryRepository();

        LibraryItem dune = LibraryItemFactory.Create(ItemKind.Book, "Dune", "Frank Herbert", copies: 3);

        repo.Add(dune);

        repo.Add(LibraryItemFactory.Create(ItemKind.Magazine, "Wired", "Axel", copies: 2));
        repo.Add(LibraryItemFactory.Create(ItemKind.Magazine, "Dune Messiah", "Frank Herbert", copies: 2));
        repo.Add(LibraryItemFactory.Create(ItemKind.ReferenceBook, "C# Language Reference", "Microsoft", copies: 3));

        Catalog catalog = new();

        foreach (LibraryItem item in repo.GetAll())
        {
            catalog.Add(item);
        }

        Console.WriteLine($"We have  {catalog.Authors.Count} unique authors in our catalog");

        foreach (string author in catalog.Authors)
        {
            Console.WriteLine(author);
        }

        // lets search out catalog now that it's backed by a dictionary
        //lets use our Find() method
        List<LibraryItem> byFrankHerbert = catalog.Find(item => item.Author == "Frank Herbert");
        Console.WriteLine($"There are {byFrankHerbert.Count} book by Frank Herbert");

        // lets se how many items in the catalog are lendable
        Console.WriteLine("We have a mix of lendable and non-lndable items");

        foreach(LibraryItem item in catalog.lendable()){
            Console.WriteLine($"{item.Title}");
        }
    }
}