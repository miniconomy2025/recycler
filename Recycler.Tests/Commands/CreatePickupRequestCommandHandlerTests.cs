using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Recycler.API.Commands.CreatePickupRequest;
using Recycler.API.Models.ExternalApiResponses;
using Xunit;

namespace Recycler.Tests.Commands
{
    public class CreatePickupRequestCommandHandlerTests
    {
        private static IConfiguration BuildConfig(string url = "http://logistics.test")
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "bulkLogisticsUrl", url }
                })
                .Build();
        }

        private static HttpClient BuildHttpClient(HttpResponseMessage response)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            return new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("http://logistics.test")
            };
        }

        private static IHttpClientFactory BuildHttpFactory(HttpClient httpClient)
        {
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
            return factory.Object;
        }

        [Fact]
        public async Task Handle_Should_Return_Success_When_LogisticsApi_Returns_ValidResponse()
        {
            // Arrange
            var logisticsResponse = new
            {
                pickupRequestId = 123,
                cost = 99.99,
                paymentReferenceId = "PAY-001",
                bulkLogisticsBankAccountNumber = "123456789",
                status = "Confirmed",
                statusCheckUrl = "http://check/status/123"
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(logisticsResponse)
            };

            var httpClient = BuildHttpClient(response);
            var factory = BuildHttpFactory(httpClient);
            var config = BuildConfig();

            var handler = new CreatePickupRequestCommandHandler(factory, config);

            var command = new CreatePickupRequestCommand
            {
                originalExternalOrderId = "ORD-001",
                originCompany = "Pear",
                destinationCompany = "BulkLogistics",
                items = new List<PickupItem>
                {
                    new() { itemName = "Copper", quantity = 2 }
                }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("Pickup request created successfully");
            result.PickupRequestId.Should().Be(123);
            result.Cost.Should().Be((decimal)99.99);
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_Api_Returns_Error()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad Request")
            };

            var httpClient = BuildHttpClient(response);
            var factory = BuildHttpFactory(httpClient);
            var config = BuildConfig();

            var handler = new CreatePickupRequestCommandHandler(factory, config);

            var command = new CreatePickupRequestCommand
            {
                originalExternalOrderId = "ORD-002",
                originCompany = "Pear",
                destinationCompany = "BulkLogistics",
                items = new List<PickupItem>
                {
                    new() { itemName = "Aluminum", quantity = 1 }
                }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Failed to create pickup request");
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_Response_Is_Null()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            };

            var httpClient = BuildHttpClient(response);
            var factory = BuildHttpFactory(httpClient);
            var config = BuildConfig();

            var handler = new CreatePickupRequestCommandHandler(factory, config);

            var command = new CreatePickupRequestCommand
            {
                originalExternalOrderId = "ORD-003",
                originCompany = "Pear",
                destinationCompany = "BulkLogistics",
                items = new List<PickupItem>
                {
                    new() { itemName = "Glass", quantity = 3 }
                }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid response from logistics API");
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_Exception_Occurs()
        {
            // Arrange: simulate network exception
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network failure"));

            var httpClient = new HttpClient(handler.Object);
            var factory = BuildHttpFactory(httpClient);
            var config = BuildConfig();

            var commandHandler = new CreatePickupRequestCommandHandler(factory, config);

            var command = new CreatePickupRequestCommand
            {
                originalExternalOrderId = "ORD-004",
                originCompany = "Pear",
                destinationCompany = "BulkLogistics",
                items = new List<PickupItem>
                {
                    new() { itemName = "Plastic", quantity = 1 }
                }
            };

            // Act
            var result = await commandHandler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Error creating pickup request");
        }
    }
}
