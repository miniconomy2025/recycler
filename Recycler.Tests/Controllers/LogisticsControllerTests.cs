using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Recycler.API.Commands;
using Recycler.API.Controllers;
using Recycler.API.Dto;
using Recycler.API.Models;
using Recycler.API.Services;
using Xunit;

namespace Recycler.Tests.Controllers
{
    public class LogisticsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogService> _logServiceMock;
        private readonly LogisticsController _controller;

        public LogisticsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _logServiceMock = new Mock<ILogService>();

            _controller = new LogisticsController(_mediatorMock.Object, _logServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        // --------------------------------------------------
        // 🟢 ProcessGeneralLogisticsEvent tests
        // --------------------------------------------------

        [Theory]
        [InlineData("PICKUP")]
        [InlineData("DELIVERY")]
        public async Task ProcessGeneralLogisticsEvent_ShouldReturnOk_ForValidType(string type)
        {
            // Arrange
            var request = new LogisticsRequestDto
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Items = new List<LogisticsItemDto> { new() { Name = "Copper", Quantity = 2 } }
            };

            var expectedResponse = new LogisticsResponseDto
            {
                Message = $"Processed {type} event successfully.",
                LogisticsRecordId = Guid.NewGuid().ToString()
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ProcessLogisticsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ProcessGeneralLogisticsEvent(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResponse);

            _mediatorMock.Verify(m => m.Send(It.IsAny<ProcessLogisticsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _logServiceMock.Verify(l => l.CreateLog(It.IsAny<HttpContext>(), request, It.IsAny<IActionResult>()), Times.Once);
        }

        [Fact]
        public async Task ProcessGeneralLogisticsEvent_ShouldReturnBadRequest_ForInvalidType()
        {
            // Arrange
            var request = new LogisticsRequestDto { Type = "SHIPMENT" };

            // Act
            var result = await _controller.ProcessGeneralLogisticsEvent(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeEquivalentTo(new { Message = "Invalid 'type' specified. Must be 'PICKUP' or 'DELIVERY'." });

            _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<LogisticsResponseDto>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessGeneralLogisticsEvent_ShouldReturn500_WhenExceptionOccurs()
        {
            // Arrange
            var request = new LogisticsRequestDto { Id = Guid.NewGuid().ToString(), Type = "PICKUP" };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ProcessLogisticsCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database down"));

            // Act
            var result = await _controller.ProcessGeneralLogisticsEvent(request);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            var value = objectResult.Value.Should().BeAssignableTo<object>().Subject;
            value.Should().BeEquivalentTo(new
            {
                Message = "An internal server error occurred while processing the logistics event.",
                Details = "Database down"
            });

            _logServiceMock.Verify(l =>
                l.CreateLog(It.IsAny<HttpContext>(), request, It.IsAny<IActionResult>()),
                Times.Once);
        }


        [Fact]
        public async Task ProcessConsumerDeliveryNotification_ShouldReturnBadRequest_WhenStatusMissing()
        {
            var dto = new ConsumerLogisticsDeliveryNotificationRequestDto
            {
                Status = null,
                ModelName = "iPhone",
                Quantity = 5
            };

            var result = await _controller.ProcessConsumerDeliveryNotification(dto);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeEquivalentTo(new { Message = "Status field is required." });
        }

        [Theory]
        [InlineData(null, 10)]
        [InlineData("", 5)]
        [InlineData("iPhone", 0)]
        [InlineData("Samsung", -1)]
        public async Task ProcessConsumerDeliveryNotification_ShouldReturnBadRequest_WhenModelNameOrQuantityInvalid(string modelName, int quantity)
        {
            var dto = new ConsumerLogisticsDeliveryNotificationRequestDto
            {
                Status = "delivered",
                ModelName = modelName,
                Quantity = quantity
            };

            var result = await _controller.ProcessConsumerDeliveryNotification(dto);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeEquivalentTo(new { Message = "ModelName and Quantity (must be > 0) are required for delivered items." });
        }

        [Fact]
        public async Task ProcessConsumerDeliveryNotification_ShouldReturnOk_ForValidRequest()
        {
            // Arrange
            var dto = new ConsumerLogisticsDeliveryNotificationRequestDto
            {
                Status = "delivered",
                ModelName = "iPhone",
                Quantity = 3
            };

            var expected = new ConsumerLogisticsDeliveryResponseDto { Message = "Phones received" };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ProcessConsumerDeliveryNotificationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.ProcessConsumerDeliveryNotification(dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expected);

            _mediatorMock.Verify(m => m.Send(It.IsAny<ProcessConsumerDeliveryNotificationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessConsumerDeliveryNotification_ShouldReturn500_WhenExceptionThrown()
        {
            // Arrange
            var dto = new ConsumerLogisticsDeliveryNotificationRequestDto
            {
                Status = "delivered",
                ModelName = "iPhone",
                Quantity = 1
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ProcessConsumerDeliveryNotificationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Network unreachable"));

            // Act
            var result = await _controller.ProcessConsumerDeliveryNotification(dto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            objectResult.Value.Should().BeEquivalentTo(new
            {
                Message = "An internal server error occurred while processing the consumer delivery notification.",
                Details = "Network unreachable"
            });
        }
    }
}
