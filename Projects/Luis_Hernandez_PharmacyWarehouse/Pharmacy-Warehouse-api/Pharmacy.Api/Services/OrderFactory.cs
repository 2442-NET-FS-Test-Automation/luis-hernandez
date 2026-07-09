using Pharmacy.Data.Entities;
using Pharmacy.Data;

namespace Pharmacy.Api.Services;

public class OrderFactory
{
    private readonly IFulfillmentService _fs;

    public OrderFactory(IFulfillmentService fulfillmentService)
    {
        _fs = fulfillmentService;
    }

    public Order CreateOrder(string kind, int customerId, IEnumerable<(string sku, int qty)> lines)
    {
        switch (kind)
        {
            case "normal":
                return BuildOrder(OrderPriority.Normal, customerId, lines);
            case "expedited":
                return BuildOrder(OrderPriority.Expedited, customerId, lines);
            default: throw new ArgumentException($"Unknown order kind: {kind}");
        }
    }

    private Order BuildOrder(OrderPriority priority, int customerId, IEnumerable<(string batch, int units)> lines)
    {
        //I need to asign a dispatcher here
        return new Order
        {
            CustomerId = customerId,
            Priority = priority,
            Status = OrderStatus.Pending,
            Lines = lines.Select(l => new OrderLine
            {
                ProductId = _fs.ResolveProductId(l.batch),
                Units = l.units
            }).ToList()
        };
    }

}