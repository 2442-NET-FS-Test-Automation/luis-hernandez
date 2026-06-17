namespace CalorieCounter.Domain;

public class Food : Aliment
{

    public Food(string name, Portion portion) : base(name, portion)
    {
    }


    public override Served ServeAliment(Double quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        Double factor = quantity / this.Portion.SuggestedAmount;

        Double calories = this.Portion.Calories * factor;
        Double protein = this.Portion.Protein * factor;
        Double carbs = this.Portion.Carbohydrates * factor;
        Double fats = this.Portion.Fats * factor;

        Served served = new Served(this, quantity, calories, protein, carbs, fats);

        return served;
    }
}