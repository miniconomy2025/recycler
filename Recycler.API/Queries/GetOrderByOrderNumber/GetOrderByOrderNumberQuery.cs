using MediatR;

namespace Recycler.API;

public record GetOrderByOrderNumberQuery(Guid OrderNumber) : IRequest<OrderDto>;