using Xunit;
using Moq;
using FluentAssertions;
using Recycler.API;
using Recycler.API.Models;
using Recycler.API.Services;

namespace Recycler.Tests.Commands
{
    public class CreateOrderCommandHandlerTests
    {

        [Fact]
        public async Task Handle_ShouldFail_WhenInsufficientStock()
        {
            // Arrange
            var company = new Company
            {
                Id = 1,
                Name = "Pear",
                Role = new Role { Id = 1, Name = "Supplier" }
            };

            var companyRepo = new Mock<IGenericRepository<Company>>();
            companyRepo.Setup(r => r.GetByColumnValueAsync("name", "Pear"))
                    .ReturnsAsync(new List<Company> { company });

            companyRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<Company> { company });

            var rawMaterial = new RawMaterial { Id = 1, Name = "Copper", PricePerKg = 50m };
            var rawMaterialDto = new RawMaterialDto { Name = "Copper", AvailableQuantityInKg = 500, PricePerKg = 50m };

            var rawMaterialService = new Mock<IRawMaterialService>();
            rawMaterialService.Setup(s => s.GetAvailableRawMaterialsAndQuantity())
                            .ReturnsAsync(new List<RawMaterialDto> { rawMaterialDto });
            rawMaterialService.Setup(s => s.GetByColumnValueAsync("name", "Copper"))
                            .ReturnsAsync(new List<RawMaterial> { rawMaterial });

            var materialRepo = new Mock<IGenericRepository<MaterialInventory>>();
            materialRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                        .ReturnsAsync(new MaterialInventory
                        {
                            Id = 1,
                            MaterialId = 1,
                            AvailableQuantityInKg = 500,
                            ReservedQuantityInKg = 0
                        });

            var handler = new CreateOrderCommandHandler(
                companyRepo.Object,
                Mock.Of<IGenericRepository<Role>>(),
                materialRepo.Object,
                rawMaterialService.Object,
                Mock.Of<ISimulationClock>(),
                Mock.Of<IGenericRepository<OrderStatus>>(),
                Mock.Of<IGenericRepository<Order>>(),
                Mock.Of<IGenericRepository<OrderItem>>(),
                Mock.Of<ICommercialBankService>()
            );

            var command = new CreateOrderCommand
            {
                CompanyName = "Pear",
                OrderItems = new List<CreateOrderItemDto>
                {
                    new() { RawMaterialName = "Copper", QuantityInKg = 2000 }
                }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("sufficient stock");
        }


        [Fact]
        public async Task Handle_ShouldFail_WhenQuantityNotMultipleOf1000()
        {
            // Arrange
            var handler = CreateHandler();

            var command = new CreateOrderCommand
            {
                CompanyName = "Pear",
                OrderItems = new List<CreateOrderItemDto>
                {
                    new() { RawMaterialName = "Cooper" , QuantityInKg = 750 } 
                }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Can only order raw materials in multiples of 1000 kg.");
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenCompanyNotFound()
        {
            // Arrange
            var companyRepo = new Mock<IGenericRepository<Company>>();
            companyRepo.Setup(r => r.GetByColumnValueAsync("name", It.IsAny<string>()))
                       .ReturnsAsync(new List<Company>());

            var allCompanies = new List<Company> { new Company { Name = "SumSang" , Role = new Role { Id=1 , Name = "Supplier" } } };
            companyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(allCompanies);

            var handler = CreateHandler(companyRepo: companyRepo);

            var command = new CreateOrderCommand
            {
                CompanyName = "Pear",
                OrderItems = new List<CreateOrderItemDto>
                {
                    new() { RawMaterialName = "Copper", QuantityInKg = 1000 }
                }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("does not exist");
        }

        [Fact]
        public async Task Handle_ShouldSucceed_WhenAllStockAvailable()
        {
            // Arrange
            var company = new Company { Id = 1, Name = "Pear" , Role = new Role { Id=1 , Name = "Supplier" }};
            var rawMaterial = new RawMaterial { Id = 10, Name = "Copper", PricePerKg = 5 };
            var inventory = new MaterialInventory { Id = 10, AvailableQuantityInKg = 5000, ReservedQuantityInKg = 0 };

            var companyRepo = new Mock<IGenericRepository<Company>>();
            companyRepo.Setup(r => r.GetByColumnValueAsync("name", "Pear"))
                       .ReturnsAsync(new List<Company> { company });

            var materialRepo = new Mock<IGenericRepository<MaterialInventory>>();
            materialRepo.Setup(r => r.GetByIdAsync(rawMaterial.Id))
                        .ReturnsAsync(inventory);

            var rawMaterialService = new Mock<IRawMaterialService>();
            rawMaterialService.Setup(s => s.GetAvailableRawMaterialsAndQuantity())
                              .ReturnsAsync(new List<RawMaterialDto> { 
                                  new RawMaterialDto 
                                  { 
                                      Name = rawMaterial.Name!, 
                                      AvailableQuantityInKg = inventory.AvailableQuantityInKg, 
                                      PricePerKg = rawMaterial.PricePerKg 
                                  } 
                              });
            rawMaterialService.Setup(s => s.GetByColumnValueAsync("name", rawMaterial.Name))
                              .ReturnsAsync(new List<RawMaterial> { rawMaterial });

            var orderStatusRepo = new Mock<IGenericRepository<OrderStatus>>();
            orderStatusRepo.Setup(r => r.GetByColumnValueAsync("name", "Pending"))
                           .ReturnsAsync(new List<OrderStatus> { new() { Id = 1, Name = "Pending" } });

            var orderRepo = new Mock<IGenericRepository<Order>>();
            orderRepo.Setup(r => r.CreateAsync(It.IsAny<Order>())).ReturnsAsync(123);

            var orderItemRepo = new Mock<IGenericRepository<OrderItem>>();

            var handler = CreateHandler(
                companyRepo: companyRepo,
                materialInventoryRepo: materialRepo,
                rawMaterialService: rawMaterialService,
                orderStatusRepo: orderStatusRepo,
                orderRepo: orderRepo,
                orderItemRepo: orderItemRepo
            );

            var command = new CreateOrderCommand
            {
                CompanyName = "Pear",
                OrderItems = new List<CreateOrderItemDto>
                {
                    new() { RawMaterialName = "Copper", QuantityInKg = 1000 }
                }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Successfully created new order");
            result.Data.Should().NotBeNull();

            materialRepo.Verify(r => r.UpdateAsync(It.IsAny<MaterialInventory>(), It.IsAny<List<string>>()), Times.Once);
            orderRepo.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Once);
            orderItemRepo.Verify(r => r.CreateAsync(It.IsAny<OrderItem>()), Times.Once);
        }

        // --- Helper method to assemble handler with mocks ---
        private static CreateOrderCommandHandler CreateHandler(
            Mock<IGenericRepository<Company>>? companyRepo = null,
            Mock<IGenericRepository<Role>>? roleRepo = null,
            Mock<IGenericRepository<MaterialInventory>>? materialInventoryRepo = null,
            Mock<IRawMaterialService>? rawMaterialService = null,
            Mock<ISimulationClock>? simulationClock = null,
            Mock<IGenericRepository<OrderStatus>>? orderStatusRepo = null,
            Mock<IGenericRepository<Order>>? orderRepo = null,
            Mock<IGenericRepository<OrderItem>>? orderItemRepo = null,
            Mock<ICommercialBankService>? bankService = null)
        {
            companyRepo ??= new Mock<IGenericRepository<Company>>();
            roleRepo ??= new Mock<IGenericRepository<Role>>();
            materialInventoryRepo ??= new Mock<IGenericRepository<MaterialInventory>>();
            rawMaterialService ??= new Mock<IRawMaterialService>();
            simulationClock ??= new Mock<ISimulationClock>();
            orderStatusRepo ??= new Mock<IGenericRepository<OrderStatus>>();
            orderRepo ??= new Mock<IGenericRepository<Order>>();
            orderItemRepo ??= new Mock<IGenericRepository<OrderItem>>();
            bankService ??= new Mock<ICommercialBankService>();

            return new CreateOrderCommandHandler(
                companyRepo.Object,
                roleRepo.Object,
                materialInventoryRepo.Object,
                rawMaterialService.Object,
                simulationClock.Object,
                orderStatusRepo.Object,
                orderRepo.Object,
                orderItemRepo.Object,
                bankService.Object
            );
        }
    }
}
