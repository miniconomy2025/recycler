using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Recycler.API.Controllers;
using Recycler.API.Services;
using Recycler.API;
using Xunit;

namespace Recycler.Tests.Controllers
{
    public class BankNotificationControllerTests
    {
        private readonly Mock<IGenericRepository<Order>> _orderRepoMock;
        private readonly Mock<IGenericRepository<OrderStatus>> _orderStatusRepoMock;
        private readonly Mock<ILogger<BankNotificationController>> _loggerMock;
        private readonly Mock<ILogService> _logServiceMock;
        private readonly BankNotificationController _controller;

        public BankNotificationControllerTests()
        {
            _orderRepoMock = new Mock<IGenericRepository<Order>>();
            _orderStatusRepoMock = new Mock<IGenericRepository<OrderStatus>>();
            _loggerMock = new Mock<ILogger<BankNotificationController>>();
            _logServiceMock = new Mock<ILogService>();

            _controller = new BankNotificationController(
                _orderRepoMock.Object,
                _orderStatusRepoMock.Object,
                _loggerMock.Object,
                _logServiceMock.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task ReceiveNotification_SuccessfulPayment_ShouldApproveOrderAndReturnOk()
        {
            // Arrange
            var orderGuid = Guid.NewGuid();
            var order = new Order
            {
                Id = 1,
                OrderNumber = orderGuid,
                OrderStatusId = 1 // Pending
            };
            var approvedStatus = new OrderStatus
            {
                Id = 2,
                Name = "Approved"
            };

            var notification = new BankNotificationDto
            {
                status = "success",
                description = orderGuid.ToString(),
                transaction_number = "TXN-12345"
            };

            _orderRepoMock
                .Setup(r => r.GetByColumnValueAsync("order_number", orderGuid))
                .ReturnsAsync(new List<Order> { order });

            _orderStatusRepoMock
                .Setup(r => r.GetByColumnValueAsync("name", "Approved"))
                .ReturnsAsync(new List<OrderStatus> { approvedStatus });

            _orderRepoMock
                .Setup(r => r.UpdateAsync(order, It.IsAny<List<string>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ReceiveNotification(notification);

            // Assert
            result.Should().BeOfType<OkResult>();
            order.OrderStatusId.Should().Be(2);

            _orderRepoMock.Verify(
                r => r.UpdateAsync(
                    It.Is<Order>(o => o.Id == 1 && o.OrderStatusId == 2),
                    It.Is<List<string>>(cols => cols.Exists(c =>
                        string.Equals(c, "OrderStatusId", StringComparison.OrdinalIgnoreCase)))),
                Times.Once);

            _logServiceMock.Verify(
                l => l.CreateLog(It.IsAny<HttpContext>(), notification, It.IsAny<IActionResult>()),
                Times.Once);
        }

        [Fact]
        public async Task ReceiveNotification_FailedPayment_ShouldIgnoreAndReturnOk()
        {
            // Arrange
            var notification = new BankNotificationDto
            {
                status = "failed",
                transaction_number = "TXN-FAILED-001",
                description = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _controller.ReceiveNotification(notification);

            // Assert
            result.Should().BeOfType<OkResult>();

            _orderRepoMock.Verify(r => r.GetByColumnValueAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            _orderRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<List<string>>()), Times.Never);

            _loggerMock.VerifyLog(LogLevel.Warning, Times.Once());
        }

        [Fact]
        public async Task ReceiveNotification_InvalidDescriptionFormat_ShouldReturnBadRequest()
        {
            var notification = new BankNotificationDto
            {
                status = "success",
                description = "not-a-valid-guid-123",
                transaction_number = "TXN-INVALID-001"
            };

            var result = await _controller.ReceiveNotification(notification);

            var badRequest = result as ObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.StatusCode.Should().Be(400);
            badRequest.Value.Should().Be("Invalid order number format");
        }

        [Fact]
        public async Task ReceiveNotification_MissingOrder_ShouldReturnNotFound()
        {
            var validGuid = Guid.NewGuid();
            var notification = new BankNotificationDto
            {
                status = "success",
                description = validGuid.ToString(),
                transaction_number = "TXN-NOTFOUND-001"
            };

            _orderRepoMock
                .Setup(r => r.GetByColumnValueAsync("order_number", validGuid))
                .ReturnsAsync(new List<Order>());

            var result = await _controller.ReceiveNotification(notification);

            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be("Order not found");
        }

        [Fact]
        public async Task ReceiveNotification_MissingApprovedStatus_ShouldReturn500()
        {
            var orderGuid = Guid.NewGuid();
            var order = new Order { Id = 1, OrderNumber = orderGuid };

            var notification = new BankNotificationDto
            {
                status = "success",
                description = orderGuid.ToString(),
                transaction_number = "TXN-NOSTATUS-001"
            };

            _orderRepoMock
                .Setup(r => r.GetByColumnValueAsync("order_number", orderGuid))
                .ReturnsAsync(new List<Order> { order });

            _orderStatusRepoMock
                .Setup(r => r.GetByColumnValueAsync("name", "Approved"))
                .ReturnsAsync(new List<OrderStatus>());

            var result = await _controller.ReceiveNotification(notification);

            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Order status configuration missing");

            _loggerMock.VerifyLog(LogLevel.Error, Times.Once());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-guid")]
        [InlineData("12345")]
        [InlineData("abc-def-ghi")]
        public async Task ReceiveNotification_InvalidGuids_ShouldReturnBadRequest(string invalidDescription)
        {
            var notification = new BankNotificationDto
            {
                status = "success",
                description = invalidDescription,
                transaction_number = "TXN-001"
            };

            var result = await _controller.ReceiveNotification(notification);

            var objectResult = result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task ReceiveNotification_UpdateThrowsException_ShouldReturn500()
        {
            var orderGuid = Guid.NewGuid();
            var order = new Order { Id = 1, OrderNumber = orderGuid };
            var approvedStatus = new OrderStatus { Id = 2, Name = "Approved" };

            var notification = new BankNotificationDto
            {
                status = "success",
                description = orderGuid.ToString(),
                transaction_number = "TXN-UPDATE-FAIL"
            };

            _orderRepoMock
                .Setup(r => r.GetByColumnValueAsync("order_number", orderGuid))
                .ReturnsAsync(new List<Order> { order });

            _orderStatusRepoMock
                .Setup(r => r.GetByColumnValueAsync("name", "Approved"))
                .ReturnsAsync(new List<OrderStatus> { approvedStatus });

            _orderRepoMock
                .Setup(r => r.UpdateAsync(order, It.IsAny<List<string>>()))
                .ThrowsAsync(new Exception("Update operation failed"));

            var result = await _controller.ReceiveNotification(notification);

            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);

            _loggerMock.VerifyLog(LogLevel.Error, Times.Once());
        }
    }

    internal static class LoggerMoqExtensions
    {
        public static void VerifyLog<T>(
            this Mock<ILogger<T>> loggerMock,
            LogLevel level,
            Times times,
            string because = "")
        {
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == level),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times,
                because);
        }
    }
}
