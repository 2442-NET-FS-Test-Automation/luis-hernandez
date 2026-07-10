namespace Pharmacy.Data;

public enum OrderStatus
{
    //A recent created order
    Pending,
    //When an order is processed correctly
    Fulfilled,
    //When an order can't be processed because of lack of inventory
    Backordered
}