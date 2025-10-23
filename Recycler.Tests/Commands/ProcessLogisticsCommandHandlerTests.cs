using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Recycler.API;
using Recycler.API.Commands;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Recycler.Tests.Commands.ProcessLogistics
{
    public class ProcessConsumerDeliveryNotificationCommandHandlerTests
    {
        private readonly IConfiguration _configuration;
        private readonly Mock<ILogger<ProcessConsumerDeliveryNotificationCommandHandler>> _mockLogger;
        private const string ConnectionString = "Host=localhost;Database=testdb;Username=test;Password=test";

        public ProcessConsumerDeliveryNotificationCommandHandlerTests()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", ConnectionString}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _mockLogger = new Mock<ILogger<ProcessConsumerDeliveryNotificationCommandHandler>>();
        }

        [Fact]
        public async Task Handle_WhenStatusIsNotDelivered_ReturnsDeliveryNotProcessedMessage()
        {
            var handler = new ProcessConsumerDeliveryNotificationCommandHandler(_configuration, _mockLogger.Object);
            var command = new ProcessConsumerDeliveryNotificationCommand
            {
                Status = "pending",
                ModelName = "iPhone 13",
                Quantity = 5
            };

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Delivery not processed", result.Message);
        }

        [Theory]
        [InlineData("pending")]
        [InlineData("cancelled")]
        [InlineData("in_transit")]
        [InlineData("")]
        public async Task Handle_WhenStatusIsNotDelivered_WithVariousStatuses_ReturnsDeliveryNotProcessedMessage(string status)
        {
            var handler = new ProcessConsumerDeliveryNotificationCommandHandler(_configuration, _mockLogger.Object);
            var command = new ProcessConsumerDeliveryNotificationCommand
            {
                Status = status,
                ModelName = "iPhone 13",
                Quantity = 5
            };

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Delivery not processed", result.Message);
        }

        [Theory]
        [InlineData("DELIVERED")]
        [InlineData("Delivered")]
        [InlineData("delivered")]
        [InlineData("DeLiVeReD")]
        public async Task Handle_WhenStatusIsDeliveredWithVariousCasing_AttemptsProcessing(string status)
        {
            var handler = new ProcessConsumerDeliveryNotificationCommandHandler(_configuration, _mockLogger.Object);
            var command = new ProcessConsumerDeliveryNotificationCommand
            {
                Status = status,
                ModelName = "iPhone 13",
                Quantity = 5
            };

            var result = await handler.Handle(command, CancellationToken.None);
            
            Assert.NotEqual("Delivery not processed", result.Message);
        }
    }
}
