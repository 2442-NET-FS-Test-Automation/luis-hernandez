namespace CalorieCounter.Domain;

public interface Cooked
{

    public Served cooking(Served served);

    public Served raw(Served served);

}