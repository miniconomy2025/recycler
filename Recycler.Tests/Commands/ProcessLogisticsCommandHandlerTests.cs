using Microsoft.Extensions.Configuration;
using Moq;
using Recycler.API;
using Recycler.API.Commands;
using MediatR;
using Xunit;

namespace Recycler.Tests.Commands.ProcessLogistics
{
        public class ProcessConsumerDeliveryNotificationCommandHandlerTests
    {
        private readonly IConfiguration _configuration;
        private const string ConnectionString = "Host=localhost;Database=testdb;Username=test;Password=test";

        public ProcessConsumerDeliveryNotificationCommandHandlerTests()
        {
            // Create a real IConfiguration with in-memory settings
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", ConnectionString}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task Handle_WhenStatusIsNotDelivered_ReturnsDeliveryNotProcessedMessage()
        {
            // Arrange
            var handler = new ProcessConsumerDeliveryNotificationCommandHandler(_configuration);
            var command = new ProcessConsumerDeliveryNotificationCommand
            {
                Status = "pending",
                ModelName = "iPhone 13",
                Quantity = 5
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
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
            // Arrange
            var handler = new ProcessConsumerDeliveryNotificationCommandHandler(_configuration);
            var command = new ProcessConsumerDeliveryNotificationCommand
            {
                Status = status,
                ModelName = "iPhone 13",
                Quantity = 5
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
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
            // Arrange
            var handler = new ProcessConsumerDeliveryNotificationCommandHandler(_configuration);
            var command = new ProcessConsumerDeliveryNotificationCommand
            {
                Status = status,
                ModelName = "iPhone 13",
                Quantity = 5
            };

            // Act 
            // This will attempt database connection and likely fail without a real database
            var result = await handler.Handle(command, CancellationToken.None);
            
            // Assert
            // Should not return "Delivery not processed" since status check passed
            Assert.NotEqual("Delivery not processed", result.Message);
        }
    }
    }

