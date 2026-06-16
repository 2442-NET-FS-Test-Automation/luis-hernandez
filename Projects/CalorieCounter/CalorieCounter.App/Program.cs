using System.Globalization;
using System.Numerics;
using CalorieCounter.Domain;

namespace CalorieCounter.App;

class Program
{
    static List<Aliment> aliments = MockAliments();
    static List<DailyLog> dailyLogs = MockDailyLogs();

    static void Main()
    {

        var running = true;
        while (running)
        {
            Console.WriteLine("\n=== Calorie Counter Menu ===");
            Console.WriteLine("1. See daily registries");
            Console.WriteLine("2. List of todays meals");
            Console.WriteLine("3. Add new meal");
            Console.WriteLine("4. Create Food");
            //Console.WriteLine("5. Create Recipe");
            Console.WriteLine("6. Exit");
            Console.Write("Choose an option: ");

            string? choice = Console.ReadLine();   // naive: may throw on bad input — fine for now
            switch (choice)
            {
                case "1":
                    ListAllRegistries();
                    ClearConsole();
                    break;
                case "2":
                    ListTodaysMeals();
                    ClearConsole();
                    break;
                case "3":
                    AddNewMeal();
                    ClearConsole();
                    break;
                case "4":
                    CreateFood();
                    ClearConsole();
                    break;
                case "5":
                    //CreateRecipe();
                    ClearConsole();
                    break;
                case "6":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option. Try again.");
                    ClearConsole();
                    break;
            }
        }
    }

    static void ClearConsole()
    {
        Console.WriteLine("\nPress enter to continue");
        Console.ReadLine();
        Console.Clear();
    }

    static void ListAllRegistries()
    {

        while (true)
        {
            Console.WriteLine("------- List of Daily Registries -------");

            for (int i = 0; i < dailyLogs.Count; i++)
            {
                Console.WriteLine($"ID {i + 1}. {dailyLogs[i].DateTime:dd/MM/yyyy}");
            }

            Console.Write("\nType ID to see details (0 for exit): ");

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                continue;
            }

            if (id == 0)
            {
                break;
            }

            if (id < 1 || id > dailyLogs.Count)
            {
                Console.WriteLine("ID not found.");
                continue;
            }

            DateTime selectedDate = dailyLogs[id - 1].DateTime;

            Console.WriteLine();
            ListAllMealsByDay(selectedDate);
            ClearConsole();
        }
    }
    static void ListTodaysMeals()
    {
        ListAllMealsByDay(DateTime.Today);
    }    
    static void CreateFood()
    {
        Console.Write("Food name: ");
        string foodName = Console.ReadLine();

        Console.Write($"-- Define a Portion for {foodName} -- \n\n");
        Console.WriteLine("Select portion type:");
        Console.WriteLine("1. Grams");
        Console.WriteLine("2. Unit");
        Console.WriteLine("3. Milliliters");

        int option = int.Parse(Console.ReadLine());

        PortionUnit unit = option switch
        {
            1 => PortionUnit.Grams,
            2 => PortionUnit.Unit,
            3 => PortionUnit.Milliliters,
            _ => throw new ArgumentException("Invalid option")
        };

        Console.Write("Suggested amount: ");
        double suggestedPortion = Double.Parse(Console.ReadLine());

        Console.Write($"-- For {suggestedPortion} {unit} of {foodName} -- \n\n");
        Console.Write("Calories: ");
        double calories = Double.Parse(Console.ReadLine());
        Console.Write("Proteins: ");
        double proteins = Double.Parse(Console.ReadLine());
        Console.Write("Carbohydrates: ");
        double carbohydrates = Double.Parse(Console.ReadLine());
        Console.Write("Fats: ");
        double fats = Double.Parse(Console.ReadLine());

        Food food = new(foodName, new Portion(unit, suggestedPortion, calories, proteins, carbohydrates, fats));

        aliments.Add(food);

        Portion portion = food.Portion;

        Console.WriteLine("New Food Added!");
        Console.WriteLine(new string('-', 50));

        string formattedPortion = $"{portion.SuggestedAmount} {portion.Unit}";

        Console.WriteLine(
            $"{"Name",-25}" +
            $"{"Suggested Portion",-25}"
        );

        Console.WriteLine(new string('-', 50));

        Console.WriteLine(
            $"{food.Name,-25}" +
            $"{formattedPortion,-25}"
        );

        Console.WriteLine(new string('-', 50));

        Console.WriteLine(
            $"{"Calories",-25}" +
            $"{portion.Calories,10:F1}"
        );

        Console.WriteLine(
            $"{"Protein",-25}" +
            $"{portion.Protein,10:F1}"
        );

        Console.WriteLine(
            $"{"Carbohydrates",-25}" +
            $"{portion.Carbohydrates,10:F1}"
        );

        Console.WriteLine(
            $"{"Fat",-25}" +
            $"{portion.Fats,10:F1}"
        );

        Console.WriteLine(new string('-', 50));

    }
    static void AddNewMeal()
    {
        DateTime today = DateTime.Now;
        Console.Write($"-- Choose a meal for today -- \n\n");
        Console.Write($"1. {Meals.Breakfast}\n");
        Console.Write($"2. {Meals.Lunch}\n");
        Console.Write($"3. {Meals.Dinner}\n");
        Console.Write($"4. {Meals.Snack}\n");
        Console.Write("Digit an option: ");
        int choice = int.Parse(Console.ReadLine());

        Meals typeOfMeal = choice switch
        {
            1 => Meals.Breakfast,
            2 => Meals.Lunch,
            3 => Meals.Dinner,
            4 => Meals.Snack
        };

        Meal meal = findMeal(DateTime.Today, typeOfMeal);


        //Add servings to this meal
        InteractiveServingForMeal(meal);
    }
    static void InteractiveServingForMeal(Meal meal)
    {
        bool stopAdding = false;

        while (stopAdding == false)
        {
            ShowMealsData(meal);

            Console.WriteLine(new string('-', 80));
            Console.WriteLine();

            Console.Write("Search an aliment by name: ");
            Food food = (Food)findAlimentByName(Console.ReadLine());

            if (food != null)
            {
                Console.Write($"Serving {food.Name} in {food.Portion.Unit}: ");
                Served served = food.ServeAliment(Double.Parse(Console.ReadLine()));
                meal.AddIngredient(served);
            }
            else
            {
                Console.Write("Aliment not found");
            }



            Console.Write("\nWant to continue adding? (Y/N)");
            string input = Console.ReadLine();
            stopAdding = (input == "Y" || input == "y") ? false : true;

        }

        ShowMealsData(meal);

    }

    static void ShowMealsData(Meal meal)
    {
        Console.WriteLine($"{meal.Type} Data");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine(
            $"{"Item",-25}" +
            $"{"Calories",10}" +
            $"{"Protein",10}" +
            $"{"Carbs",10}" +
            $"{"Fat",10}"
        );

        Console.WriteLine(new string('-', 80));

        // Totales del meal
        Console.WriteLine(
            $"{"TOTAL",-25}" +
            $"{meal.Calories,10:F1}" +
            $"{meal.Protein,10:F1}" +
            $"{meal.Carbohydrates,10:F1}" +
            $"{meal.Fats,10:F1}"
        );

        // Ingredientes/servings
        foreach (Served s in meal.Ingredients)
        {
            Console.WriteLine(
                $"{s.Aliment.Name,-25}" +
                $"{s.Calories,10:F1}" +
                $"{s.Protein,10:F1}" +
                $"{s.Carbohydrates,10:F1}" +
                $"{s.Fats,10:F1}"
            );
        }
    }

    static Aliment findAlimentByName(string name)
    {
        foreach (Aliment aliment in aliments)
        {
            if (aliment.Name.ToLower() == name.ToLower())
            {
                return aliment;
            }
        }

        return null;
    }

    static Meal findMeal(DateTime dateTime, Meals typeOfMeal)
    {
        DailyLog dailyLog = findDailyLog(dateTime);

        foreach (Meal meal in dailyLog.Meals)
        {
            if (meal.Type == typeOfMeal)
            {
                return meal;
            }
        }

        Meal newMeal = new Meal(typeOfMeal);

        dailyLog.AddMeal(newMeal);

        return newMeal;
    }

    static DailyLog findDailyLog(DateTime dateTime)
    {
        foreach (DailyLog dl in dailyLogs)
        {
            if (dl.DateTime == dateTime)
            {
                return dl;
            }
        }

        DailyLog dailyLog = new DailyLog(dateTime);

        dailyLogs.Add(dailyLog);

        return dailyLog;
    }

    static void ListAllMealsByDay(DateTime dateTime)
    {
        DailyLog? dailyLog = findDailyLog(dateTime);

        if (dailyLog == null)
        {
            Console.WriteLine($"No daily log found for {dateTime:dd/MM/yyyy}");
            return;
        }

        Console.WriteLine($"Meals for {dateTime:dd/MM/yyyy}");
        Console.WriteLine();

        Meal? breakfast = findMeal(dateTime, Meals.Breakfast);
        Meal? lunch = findMeal(dateTime, Meals.Lunch);
        Meal? dinner = findMeal(dateTime, Meals.Dinner);
        Meal? snack = findMeal(dateTime, Meals.Snack);

        if (breakfast != null)
            ShowMealsData(breakfast);
            Console.WriteLine();

        if (lunch != null)
            ShowMealsData(lunch);
            Console.WriteLine();

        if (dinner != null)
            ShowMealsData(dinner);
            Console.WriteLine();

        if (snack != null)
            ShowMealsData(snack);
            Console.WriteLine();
    }

    static List<Aliment> MockAliments()
    {
        return new List<Aliment>
        {
        new Food(
            "Chicken Breast",
            new Portion(
                PortionUnit.Grams,
                100,
                165,
                31,
                0,
                3.6
            )
        ),

        new Food(
            "White Rice",
            new Portion(
                PortionUnit.Grams,
                100,
                130,
                2.7,
                28,
                0.3
            )
        ),

        new Food(
            "Egg",
            new Portion(
                PortionUnit.Unit,
                1,
                70,
                6,
                0.6,
                5
            )
        ),

        new Food(
            "Oatmeal",
            new Portion(
                PortionUnit.Grams,
                50,
                190,
                6.5,
                33,
                3.5
            )
        ),

        new Food(
            "Banana",
            new Portion(
                PortionUnit.Unit,
                1,
                105,
                1.3,
                27,
                0.4
            )
        ),

        new Food(
            "Avocado",
            new Portion(
                PortionUnit.Grams,
                100,
                160,
                2,
                9,
                15
            )
        ),

        new Food(
            "Salmon",
            new Portion(
                PortionUnit.Grams,
                100,
                208,
                20,
                0,
                13
            )
        ),

        new Food(
            "Greek Yogurt",
            new Portion(
                PortionUnit.Grams,
                170,
                100,
                17,
                6,
                0
            )
        ),

        new Food(
            "Peanut Butter",
            new Portion(
                PortionUnit.Grams,
                32,
                188,
                8,
                6,
                16
            )
        ),

        new Food(
            "Broccoli",
            new Portion(
                PortionUnit.Grams,
                100,
                34,
                2.8,
                7,
                0.4
            )
        )
        };
    }

    static List<DailyLog> MockDailyLogs()
    {
        var foods = aliments
            .ToDictionary(f => f.Name);

        var logs = new List<DailyLog>();

        DateTime startDate = new(2026, 6, 8);

        for (int day = 0; day < 5; day++)
        {
            DailyLog dailyLog = new(startDate.AddDays(day));

            // BREAKFAST
            Meal breakfast = new(Meals.Breakfast);

            breakfast.AddIngredient(
                foods["Oatmeal"].ServeAliment(50 + day * 5));

            breakfast.AddIngredient(
                foods["Banana"].ServeAliment(1));

            breakfast.AddIngredient(
                foods["Peanut Butter"].ServeAliment(32));

            // LUNCH
            Meal lunch = new(Meals.Lunch);

            lunch.AddIngredient(
                foods["Chicken Breast"].ServeAliment(150 + day * 10));

            lunch.AddIngredient(
                foods["White Rice"].ServeAliment(180));

            lunch.AddIngredient(
                foods["Broccoli"].ServeAliment(100));

            // SNACK
            Meal snack = new(Meals.Snack);

            snack.AddIngredient(
                foods["Greek Yogurt"].ServeAliment(170));

            snack.AddIngredient(
                foods["Banana"].ServeAliment(1));

            // DINNER
            Meal dinner = new(Meals.Dinner);

            dinner.AddIngredient(
                foods["Salmon"].ServeAliment(180));

            dinner.AddIngredient(
                foods["Avocado"].ServeAliment(80));

            dinner.AddIngredient(
                foods["Broccoli"].ServeAliment(120));

            dailyLog.AddMeal(breakfast);
            dailyLog.AddMeal(lunch);
            dailyLog.AddMeal(snack);
            dailyLog.AddMeal(dinner);

            logs.Add(dailyLog);
        }

        return logs;
    }
}