namespace CalorieCounter.Domain;

public class Meal
{
    private readonly List<Served> _ingredients = new();

    public Meals Type { get; }

    public IReadOnlyCollection<Served> Ingredients =>
        _ingredients.AsReadOnly();

    public double Calories =>
        _ingredients.Sum(i => i.Calories);

    public double Protein =>
        _ingredients.Sum(i => i.Protein);

    public double Carbohydrates =>
        _ingredients.Sum(i => i.Carbohydrates);

    public double Fats =>
        _ingredients.Sum(i => i.Fats);

    public Meal(Meals type)
    {
        Type = type;
    }

    public void AddIngredient(Served served)
    {
        ArgumentNullException.ThrowIfNull(served);

        _ingredients.Add(served);
    }

    public void RemoveIngredient(Served served)
    {
        ArgumentNullException.ThrowIfNull(served);

        _ingredients.Remove(served);
    }
}