using MediatR;

namespace Recycler.API;

public record GetOrderByIdQuery(int Id) : IRequest<OrderDto>;