using Library.Data.Entities;

namespace Library.Api.Fulfillment;

public class BurstPlanner
{
    // Method to plan de fulfillment order
    public IReadOnlyList<int> OrderByPriority(IEnumerable<Order> orders)
    {
        //We could make our own custom implementation on this -  we won't
        // We can use a PriorityQueue - allows for FIFO processing with priority taken into account
        //First int = orderId, second int = priority.
        //we are going to use lower number = higher priority
        //
        PriorityQueue<int, int> pq = new PriorityQueue<int, int>();

        foreach (Order o in orders)
        {
            // Enqueue each order, if it's Priority is expedited, give it a 0 value, if normal give it
            pq.Enqueue(o.Id, o.Priority == Priority.Expedited ? 0 : 1);
        }

        //This list will hold everything we want to process already in order to pass to out other method
        var orderByPriority = new List<int>();

        //While our PriorityQueue has stuff in it - loop and add those things in the order they exit to out orderedByPriority list - uses our params
        while (pq.TryDequeue(out int id, out _))
        {
            orderByPriority.Add(id);
        }

        return orderByPriority; //expedited ids should be first in the list
    }
}