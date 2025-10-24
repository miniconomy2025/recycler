using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Recycler.API.Services;
using Recycler.API.Utils; 
using Recycler.API.Models;
using Recycler.API.Models.ExternalApiRequests; 
using Xunit;

namespace Recycler.Tests.Services
{
    public class ConsumerLogisticsServiceTests
    {
        private readonly Mock<IHttpClientFactory> _clientFactoryMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly ConsumerLogisticsService _service;
        private readonly HttpClient _httpClient;

        public ConsumerLogisticsServiceTests()
        {
            _clientFactoryMock = new Mock<IHttpClientFactory>();
            _configMock = new Mock<IConfiguration>();
            _handlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://dummy.url/")
            };

            // Setup IHttpClientFactory to return our mock client
            _clientFactoryMock
                .Setup(f => f.CreateClient("test"))
                .Returns(_httpClient);

            // Setup Configuration
            _configMock.SetupGet(c => c["consumerLogistic"]).Returns("http://logistics-api.com");

            _service = new ConsumerLogisticsService(_clientFactoryMock.Object, _configMock.Object);
        }

        [Fact]
        public async Task SendDeliveryOrderAsync_ShouldReturnSuccessResponse_WhenApiReturns200()
        {
            // Arrange
    
            var order = new DeliveryOrderRequestDto { modelName = "Plastic", quantity = 100 };
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"message\":\"Pickup scheduled\"}")
            };
            
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri!.ToString().Contains("/api/pickups") && 
                        req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(expectedResponse)
                .Verifiable();

            // Act
            var result = await _service.SendDeliveryOrderAsync(order);

            // Assert
            result.Should().Be(expectedResponse);
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendDeliveryOrderAsync_ShouldThrowHttpRequestException_WhenApiReturnsNonSuccessCode()
        {
            // Arrange
           
            var order = new DeliveryOrderRequestDto { modelName = "Plastic", quantity = 100 };
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Invalid request data")
            };
            
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(expectedResponse);

            // Act & Assert
            await _service
                .Invoking(s => s.SendDeliveryOrderAsync(order))
                .Should().ThrowAsync<HttpRequestException>()
                .WithMessage("*Response status code does not indicate success*");
        }

        [Fact]
        public async Task SendDeliveryOrderAsync_ShouldThrowOriginalException_WhenConnectionFails()
        {
            // Arrange
            var order = new DeliveryOrderRequestDto { modelName = "Plastic", quantity = 100 };
            
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new TimeoutException("Connection timed out."));

            // Act & Assert
            await _service
                .Invoking(s => s.SendDeliveryOrderAsync(order))
                .Should().ThrowAsync<TimeoutException>()
                .WithMessage("Connection timed out.");
        }
    }
}