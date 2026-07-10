namespace Pharmacy.Api.DTO;

public record OrderRequest(string Kind,
    int CustomerId,
    List<OrderLineRequest> OrderLines);