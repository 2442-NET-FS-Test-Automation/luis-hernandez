namespace CalorieCounter.Domain;

public class Served
{
    public Aliment Aliment { get; }

    public double Quantity { get; }

    public double Calories { get; }

    public double Protein { get; }

    public double Carbohydrates { get; }

    public double Fats { get; }

    public Served(
        Aliment aliment,
        double quantity,
        double calories,
        double protein,
        double carbohydrates,
        double fats)
    {
        ArgumentNullException.ThrowIfNull(aliment);

        if (quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        Aliment = aliment;
        Quantity = quantity;
        Calories = calories;
        Protein = protein;
        Carbohydrates = carbohydrates;
        Fats = fats;
    }
}