using MediatR;

namespace Recycler.API;

public class GetOrderByOrderNumberHandler(
    IGenericRepository<Order> orderRepository,
    IGenericRepository<OrderStatus> orderStatusRepository,
    IGenericRepository<OrderItem> orderItemRepository) 
    : IRequestHandler<GetOrderByOrderNumberQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByOrderNumberQuery request, CancellationToken cancellationToken)
    {
        var order = (await orderRepository.GetByColumnValueAsync("order_number", request.OrderNumber)).FirstOrDefault();

        if (order is null)
        {
            throw new Exception($"Order with order number {request.OrderNumber} does not exist");
        }

        var orderStatus = await orderStatusRepository.GetByIdAsync(order.OrderStatusId);
        var orderItems = await orderItemRepository.GetByColumnValueAsync("order_id", order.Id);
        
        return new OrderDto().MapDbObjects(order, orderStatus, orderItems);
    }
}