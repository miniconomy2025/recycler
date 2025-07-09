using MediatR;
using Recycler.API.Models;

namespace Recycler.API;

public class CreateOrderCommand : IRequest<GenericResponse<OrderDto>>
{
    public required string CompanyName { get; set; }
    public required IEnumerable<CreateOrderItemDto> OrderItems { get; set; }
}