// If I have code from another namespace that I wannt to use her - I use a using satatement to import it.
using Library.Domain;
using LibraryKata.Domain;

namespace LibraryKata.App; // A logical container for different related code files.

public class Program
{

    // public - accessible across the program
    // static - Main can be called upon without a Program object. It is a Static/class method. 
    // void - it doesn't return anything
    public static void Main()
    {
        Program.DataTypesAndOperators();
        Program.ControlFlow();
        Program.Loops();
        Program.ArraysWork();
        Program.ClassesExample();
        Program.OopDemo();
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
}