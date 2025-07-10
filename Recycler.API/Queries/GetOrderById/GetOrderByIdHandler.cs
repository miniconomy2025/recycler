using MediatR;
using Recycler.API.Services;

namespace Recycler.API;

public class GetOrderByOrderIdHandler(
    IGenericRepository<Order> orderRepository,
    IGenericRepository<OrderStatus> orderStatusRepository,
    IGenericRepository<OrderItem> orderItemRepository,
    ISimulationClock simulationClock,
    IGenericRepository<RawMaterial> rawMaterialRepository,
    ICommercialBankService commercialBankService) 
    : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id);

        if (order is null)
        {
            throw new Exception($"Order with ID {request.Id} does not exist");
            // ToDo: Log instead of throw error
        }

        var orderStatus = await orderStatusRepository.GetByIdAsync(order.OrderStatusId);
        var orderItems = await orderItemRepository.GetByColumnValueAsync("order_id", order.Id);
        
        var orderItemsDto = new List<OrderItemDto>();

        foreach (var orderItem in orderItems)
        {
            var rawMaterial = await rawMaterialRepository.GetByIdAsync(orderItem.MaterialId);
            
            orderItemsDto.Add(new OrderItemDto().MapDbObjects(orderItem, rawMaterial));
        }
        
        return new OrderDto(simulationClock, commercialBankService).MapDbObjects(order, orderStatus, orderItemsDto);
    }
}