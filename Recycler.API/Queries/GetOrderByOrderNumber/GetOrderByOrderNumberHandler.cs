using MediatR;
using Recycler.API.Services;

namespace Recycler.API;

public class GetOrderByOrderNumberHandler(
    IGenericRepository<Order> orderRepository,
    IGenericRepository<OrderStatus> orderStatusRepository,
    IGenericRepository<OrderItem> orderItemRepository,
    ISimulationClock simulationClock,
    IGenericRepository<RawMaterial> rawMaterialRepository,
    ICommercialBankService commercialBankService) 
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
        
        var orderItemsDto = new List<OrderItemDto>();

        foreach (var orderItem in orderItems)
        {
            var rawMaterial = await rawMaterialRepository.GetByIdAsync(orderItem.MaterialId);
            
            orderItemsDto.Add(new OrderItemDto().MapDbObjects(orderItem, rawMaterial));
        }
        
        return new OrderDto(simulationClock, commercialBankService).MapDbObjects(order, orderStatus, orderItemsDto);
    }
}