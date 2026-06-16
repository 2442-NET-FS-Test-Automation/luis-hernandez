namespace CalorieCounter.Domain;

public class DailyLog
{
    private readonly List<Meal> _meals = new();

    public DateTime DateTime { get; }

    public IReadOnlyCollection<Meal> Meals =>
        _meals.AsReadOnly();

    public double Calories =>
        _meals.Sum(m => m.Calories);

    public double Protein =>
        _meals.Sum(m => m.Protein);

    public double Carbohydrates =>
        _meals.Sum(m => m.Carbohydrates);

    public double Fats =>
        _meals.Sum(m => m.Fats);

    public DailyLog(DateTime dateTime)
    {
        DateTime = dateTime.Date;
    }

    public void AddMeal(Meal meal)
    {
        ArgumentNullException.ThrowIfNull(meal);

        _meals.Add(meal);
    }

    public void RemoveMeal(Meal meal)
    {
        ArgumentNullException.ThrowIfNull(meal);

        _meals.Remove(meal);
    }
}