using MediatR;
using Recycler.API.Models;
using Recycler.API.Services;

namespace Recycler.API;

public class CreateOrderCommandHandler(
    IGenericRepository<Company> companyRepository,
    IGenericRepository<Role> roleRepository,
    IGenericRepository<MaterialInventory> materialInventoryRepository,
    IRawMaterialService rawMaterialService,
    ISimulationClock simulationClock,
    IGenericRepository<OrderStatus> orderStatusRepository,
    IGenericRepository<Order> orderRepository,
    IGenericRepository<OrderItem> orderItemRepository,
    ICommercialBankService commercialBankService) : IRequestHandler<CreateOrderCommand, GenericResponse<OrderDto>>
{
    public async Task<GenericResponse<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var unavailableRawMaterials = new List<string>();
        var stockToReserve = new Dictionary<MaterialInventory, OrderItemDto>();
        
        var company = (await companyRepository.GetByColumnValueAsync("name", request.CompanyName)).FirstOrDefault()
            ?? throw new Exception($"Company with name {request.CompanyName} does not exist");
            // ToDo: Log instead of throw error
            
        // ToDo: enums for companies?

        
        var availableRawMaterials = (await rawMaterialService.GetAvailableRawMaterialsAndQuantity()).ToList();

        var availableOrderItems = new List<OrderItemDto>();
        
        if (availableRawMaterials.All(rawMaterial => rawMaterial.AvailableQuantityInKg == 0))
        {
            request.OrderItems.ToList().ForEach(orderItem =>
                unavailableRawMaterials.Add(orderItem.RawMaterialName));
        }
        else
        {
            foreach (var orderItem in request.OrderItems)
            {
                var rawMaterial = (await rawMaterialService.GetByColumnValueAsync("name", orderItem.RawMaterialName))
                    .FirstOrDefault();
                var materialInventory = await materialInventoryRepository.GetByIdAsync(rawMaterial.Id);

                if (rawMaterial == null ||
                    materialInventory.AvailableQuantityInKg < orderItem.QuantityInKg)
                {
                    unavailableRawMaterials.Add(orderItem.RawMaterialName);
                }
                else
                {
                    var availableOrderItem = new OrderItemDto()
                    { 
                        RawMaterial = rawMaterial,
                        QuantityInKg = orderItem.QuantityInKg, 
                        PricePerKg = rawMaterial.PricePerKg
                    };
                    
                    stockToReserve.Add(materialInventory, availableOrderItem);
                    // availableOrderItems.Add(availableOrderItem);
                }

            }
        }

        Order? createdOrder = null;
        OrderStatus? orderStatus = null;
        bool orderSuccess;
        string orderMessage;

        if (unavailableRawMaterials.Any())
        {
            orderSuccess = false;
            orderMessage = $"Unavailable Raw Materials: {string.Join(", ", unavailableRawMaterials)}";
        }
        else
        {
            orderStatus = (await orderStatusRepository.GetByColumnValueAsync("name", "Pending")).FirstOrDefault();

            createdOrder = new Order()
            {
                OrderNumber = Guid.NewGuid(),
                OrderStatusId = orderStatus!.Id,
                CreatedAt = DateTime.Now,
                CompanyId = company.Id,
                OrderExpiresAt = DateTime.Now.AddMinutes(2)
            };
            
            var createdOrderId = await orderRepository.CreateAsync(createdOrder);
            
            createdOrder.Id = createdOrderId;
            
            foreach (var materialInventory in stockToReserve.Keys)
            {
                materialInventory.AvailableQuantityInKg -= stockToReserve[materialInventory].QuantityInKg;
                materialInventory.ReservedQuantityInKg += stockToReserve[materialInventory].QuantityInKg;
                
                await materialInventoryRepository.UpdateAsync(materialInventory, 
                    new List<string> {"AvailableQuantityInKg", "ReservedQuantityInKg"});

                var createdOrderItem = new OrderItem().MapDbObjects(stockToReserve[materialInventory]);
            
                createdOrderItem.OrderId = createdOrderId;
                
                await orderItemRepository.CreateAsync(createdOrderItem);
            }
            
            availableOrderItems = stockToReserve.Values.ToList();

            orderSuccess = true;
            orderMessage = "Successfully created new order";
            
        }

        return new GenericResponse<OrderDto>(simulationClock)
        {
            Data = new OrderDto(simulationClock, commercialBankService).MapDbObjects(createdOrder,
                orderStatus,
                availableOrderItems),
            IsSuccess = orderSuccess,
            Message = orderMessage,
            TimeStamp = simulationClock.GetCurrentSimulationTime()
        };
    }
}