namespace Pharmacy.Api.Services;

// Requests are either Fulfilled or Backordered - no other results possible
public enum FulfillmentResult { Fulfilled, Backordered }

// Resultado de procesar varias órdenes en ráfaga.
// Queued: órdenes que no consiguieron un Dispatcher libre de inmediato y
// se reencolaron en PriorityOrderQueue para el flujo normal.
public record BurstResult(int Fulfilled, int Backordered);