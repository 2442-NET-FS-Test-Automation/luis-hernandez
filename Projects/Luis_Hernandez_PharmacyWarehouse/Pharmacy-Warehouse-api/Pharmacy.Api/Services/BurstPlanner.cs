using Pharmacy.Data.Entities;
using Pharmacy.Data;

namespace Pharmacy.Api.Services;

public class BurstPlanner
{
    // Method to plan the fulfillment order
    public IReadOnlyList<Order> OrderByPriority(IEnumerable<Order> orders)
    {
        return orders
            .OrderBy(o => o.Priority == OrderPriority.Expedited ? 0 : 1)
            .ThenBy(o => o.CreatedUtc)
            .ToList();
    }
}