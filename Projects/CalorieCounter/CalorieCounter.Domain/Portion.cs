namespace CalorieCounter.Domain;

public class Portion
{
    public PortionUnit Unit { get; private set; }
    public double SuggestedAmount { get; private set; }
    public double Calories { get; private set; }
    public double Protein { get; private set; }
    public double Carbohydrates { get; private set; }
    public double Fats { get; private set; }

    public Portion(PortionUnit unit, double suggestedAmount, double calories, double protein, double carbohydrates, double fats)
    {
        Unit = unit;
        SuggestedAmount = suggestedAmount;
        Calories = calories;
        Protein = protein;
        Carbohydrates = carbohydrates;
        Fats = fats;
    }
}