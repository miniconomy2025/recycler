using MediatR;

namespace Recycler.API;

public class CreateOrderCommand : IRequest<Order>
{
    public required int SupplierId { get; set; }
    public required CreateOrderItemDto[] OrderItems { get; set; }
}