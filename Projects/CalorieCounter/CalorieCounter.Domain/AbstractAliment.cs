namespace CalorieCounter.Domain;

public abstract class Aliment
{
    public string Name { get; private set; }
    public Portion Portion { get; private set; }

    protected Aliment(string name, Portion portion)
    {
        Name = name;
        Portion = portion;
    }

    public abstract Served ServeAliment(Double quantity);
}
