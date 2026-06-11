namespace LibraryKata.App; // A logical container for different related code files.
public class Program
{
 
    // public - accessible across the program
    // static - Main can be called upon without a Program object. It is a Static/class method. 
    // void - it doesn't return anything
    public static void Main()
    {   
        Program.DataTypesAndOperators();
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

}