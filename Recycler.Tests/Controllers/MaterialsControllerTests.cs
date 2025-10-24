using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MediatR;
using Recycler.API.Controllers;
using Recycler.API.Dto;
using Recycler.API;
using Recycler.API.Services;
using Recycler.API.Queries.GetMaterials;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Recycler.Tests.Controllers
{
  
    public class MaterialsController(IMediator mediator, ILogService logService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(List<RawMaterialDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMaterials()
        {
            try
            {
                var query = new GetMaterialsQuery();
                // Assumes mediator returns List<RawMaterialDto>
                var result = await mediator.Send(query); 
                
                var response = Ok(result);
                await logService.CreateLog(HttpContext, null, response);
                
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = BadRequest(ex.Message);
                await logService.CreateLog(HttpContext, null, errorResponse);
                return errorResponse;
            }
        }
    }


    public class MaterialsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogService> _mockLogService;
        private readonly MaterialsController _controller;

        public MaterialsControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogService = new Mock<ILogService>();

            _controller = new MaterialsController(_mockMediator.Object, _mockLogService.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

        [Fact]
        public async Task GetMaterials_ReturnsOkWithMaterialsList_WhenQuerySucceeds()
        {
            // Arrange
            var expectedMaterials = new List<RawMaterialDto>
            {
                // FIX: Property names used in DTO initialization
                new RawMaterialDto { Name = "Copper", PricePerKg = 10m, AvailableQuantityInKg = 50.0 },
                new RawMaterialDto { Name = "Gold", PricePerKg = 500m, AvailableQuantityInKg = 0.0 }
            };

           
            _mockMediator.Setup(m => m.Send(
                It.IsAny<GetMaterialsQuery>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedMaterials);

            // Act
            var result = await _controller.GetMaterials();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedMaterials = okResult.Value.Should().BeAssignableTo<List<RawMaterialDto>>().Subject;
            
            returnedMaterials.Should().HaveCount(2);
            
            returnedMaterials.First().Name.Should().Be("Copper"); 
            
         
            _mockLogService.Verify(l => l.CreateLog(
                It.IsAny<HttpContext>(), 
                null, 
                It.IsAny<OkObjectResult>()), 
                Times.Once);
        }

        [Fact]
        public async Task GetMaterials_ReturnsBadRequest_WhenQueryThrowsException()
        {
            // Arrange
            var exceptionMessage = "Database connection failed.";
            
           
            _mockMediator.Setup(m => m.Send(
                It.IsAny<GetMaterialsQuery>(), 
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await _controller.GetMaterials();

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be(exceptionMessage);
            
            
            _mockLogService.Verify(l => l.CreateLog(
                It.IsAny<HttpContext>(), 
                null, 
                It.IsAny<BadRequestObjectResult>()), 
                Times.Once);
        }
    }
}