namespace CalorieCounter.Domain;

public class Food : Aliment, Cooked
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

    public Served cooking(Served served)
    {
        //some questions about cooked portions of food

        return served;
    }

    public Served raw(Served served)
    {
        //some questions about raw portions of food

        return served;
    }
}