using MediatR;

namespace Recycler.API;

public record GetOrderByOrderNumberQuery(string OrderNumber) : IRequest<Order>;