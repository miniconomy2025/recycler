using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Recycler.API.Services;
using Recycler.API.Dto;
using Recycler.API.Models; 
using Recycler.API; 
using Xunit;

namespace Recycler.Tests.Services
{
    public class RawMaterialServiceTests
    {
       
        private readonly Mock<Recycler.API.IGenericRepository<RawMaterial>> _rawMaterialRepoMock;
        private readonly Mock<Recycler.API.IGenericRepository<MaterialInventory>> _materialInventoryRepoMock;
        private readonly RawMaterialService _service;

        public RawMaterialServiceTests()
        {
        
            _rawMaterialRepoMock = new Mock<Recycler.API.IGenericRepository<RawMaterial>>();
            _materialInventoryRepoMock = new Mock<Recycler.API.IGenericRepository<MaterialInventory>>();
            
            _service = new RawMaterialService(_rawMaterialRepoMock.Object, _materialInventoryRepoMock.Object);
        }


        [Fact]
        public async Task UpdateRawMaterialPrice_ShouldCreateNewMaterial_WhenNotFound()
        {
            // Arrange
            var newMaterial = new RawMaterial { Name = "NewGold", PricePerKg = 500m };
            var materialsToUpdate = new List<RawMaterial> { newMaterial };

            _rawMaterialRepoMock
                .Setup(r => r.GetByColumnValueAsync("name", "NewGold"))
                .ReturnsAsync(Enumerable.Empty<RawMaterial>()); // Material not found

            // Act
            await _service.UpdateRawMaterialPrice(materialsToUpdate);

            // Assert
            _rawMaterialRepoMock.Verify(
                r => r.CreateAsync(It.Is<RawMaterial>(m => m.Name == "NewGold" && m.PricePerKg == 500m)),
                Times.Once);
            _rawMaterialRepoMock.Verify(r => r.UpdateAsync(It.IsAny<RawMaterial>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRawMaterialPrice_ShouldUpdatePrice_WhenPriceChanged()
        {
            // Arrange
            var existingMaterial = new RawMaterial { Id = 1, Name = "Copper", PricePerKg = 10m };
            var updatedMaterial = new RawMaterial { Name = "Copper", PricePerKg = 12m };
            var materialsToUpdate = new List<RawMaterial> { updatedMaterial };

            _rawMaterialRepoMock
                .Setup(r => r.GetByColumnValueAsync("name", "Copper"))
                .ReturnsAsync(new List<RawMaterial> { existingMaterial });

            // Act
            await _service.UpdateRawMaterialPrice(materialsToUpdate);

            // Assert
            _rawMaterialRepoMock.Verify(r => r.CreateAsync(It.IsAny<RawMaterial>()), Times.Never);
            _rawMaterialRepoMock.Verify(
                r => r.UpdateAsync(
                    It.Is<RawMaterial>(m => m.Id == 1 && m.PricePerKg == 12m),
                    It.Is<List<string>>(p => p.Contains("PricePerKg") && p.Count == 1)
                ),
                Times.Once);
        }

        [Fact]
        public async Task UpdateRawMaterialPrice_ShouldDoNothing_WhenPriceUnchanged()
        {
            // Arrange
            var existingMaterial = new RawMaterial { Id = 1, Name = "Copper", PricePerKg = 10m };
            var updatedMaterial = new RawMaterial { Name = "Copper", PricePerKg = 10m };
            var materialsToUpdate = new List<RawMaterial> { updatedMaterial };

            _rawMaterialRepoMock
                .Setup(r => r.GetByColumnValueAsync("name", "Copper"))
                .ReturnsAsync(new List<RawMaterial> { existingMaterial });

            // Act
            await _service.UpdateRawMaterialPrice(materialsToUpdate);

            // Assert
            _rawMaterialRepoMock.Verify(r => r.CreateAsync(It.IsAny<RawMaterial>()), Times.Never);
            _rawMaterialRepoMock.Verify(r => r.UpdateAsync(It.IsAny<RawMaterial>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        

        [Fact]
        public async Task GetAvailableRawMaterialsAndQuantity_ShouldReturnCorrectDto_WithInventory()
        {
            // Arrange
            var rawMaterials = new List<RawMaterial>
            {
                new RawMaterial { Id = 101, Name = "Copper", PricePerKg = 10m },
                new RawMaterial { Id = 102, Name = "Gold", PricePerKg = 500m }
            };
            
            var inventory1 = new MaterialInventory { Id = 101, AvailableQuantityInKg = 50 };
            
            _rawMaterialRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rawMaterials);
            
            _materialInventoryRepoMock.Setup(r => r.GetByIdAsync(101)).ReturnsAsync(inventory1);
           
            _materialInventoryRepoMock.Setup(r => r.GetByIdAsync(102)).ReturnsAsync((MaterialInventory?)null);

            // Act
            var result = (await _service.GetAvailableRawMaterialsAndQuantity()).ToList();

            // Assert
            result.Should().HaveCount(2);
            
            result.Should().ContainEquivalentOf(new RawMaterialDto
            {
                Name = "Copper",
                
                AvailableQuantityInKg = 50.0, 
                PricePerKg = 10m
            });
            
            result.Should().ContainEquivalentOf(new RawMaterialDto
            {
                Name = "Gold",
                
                AvailableQuantityInKg = 0.0, 
                PricePerKg = 500m
            });
        }
        
        [Fact]
        public async Task GetAvailableRawMaterialsAndQuantity_ShouldReturnEmptyList_WhenNoRawMaterialsExist()
        {
            // Arrange
            _rawMaterialRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(Enumerable.Empty<RawMaterial>());

            // Act
            var result = (await _service.GetAvailableRawMaterialsAndQuantity()).ToList();

            // Assert
            result.Should().BeEmpty();
            _materialInventoryRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
    }
}