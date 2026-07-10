using Pharmacy.Data;

namespace Pharmacy.Api.DTO;

public record SeededOrder(
    int Id,
    OrderPriority Priority
);