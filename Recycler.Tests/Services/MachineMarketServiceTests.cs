using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Recycler.API.Services;
using Recycler.API.Utils; 
using Recycler.API.Models;
using Xunit;
using static Recycler.API.Services.MachineMarketService;



namespace Recycler.Tests.Services
{
    public class MachineMarketServiceTests
    {
        private readonly Mock<IHttpClientFactory> _clientFactoryMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<MachineMarketService>> _loggerMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly MachineMarketService _service;
        private const string BaseUrl = "http://thoh-api.com/";

        public MachineMarketServiceTests()
        {
            _clientFactoryMock = new Mock<IHttpClientFactory>();
            _configMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<MachineMarketService>>();
            _handlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_handlerMock.Object);

            _clientFactoryMock.Setup(f => f.CreateClient("test")).Returns(_httpClient);
            _configMock.SetupGet(c => c["thoHApiUrl"]).Returns(BaseUrl);

            _service = new MachineMarketService(_clientFactoryMock.Object, _configMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetRecyclingMachineAsync_ShouldReturnMachine_WhenFound()
        {
            // Arrange
            var recyclingMachine = new MachineDto { machineName = "recycling_machine", price = 1000m, quantity = 5 };
            var otherMachine = new MachineDto { machineName = "sorter", price = 500m, quantity = 1 };
            
            var marketResponse = new MachineMarketResponse { machines = new List<MachineDto> { otherMachine, recyclingMachine } };
            var responseJson = JsonSerializer.Serialize(marketResponse);
            
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().Contains("/api/machines")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _service.GetRecyclingMachineAsync(CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(recyclingMachine);
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce); // Verify logging happened
        }

        [Fact]
        public async Task GetRecyclingMachineAsync_ShouldReturnNull_WhenRecyclingMachineNotFound()
        {
            // Arrange
            var otherMachine = new MachineDto { machineName = "sorter", price = 500m, quantity = 1 };
            var marketResponse = new MachineMarketResponse { machines = new List<MachineDto> { otherMachine } };
            var responseJson = JsonSerializer.Serialize(marketResponse);
            
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _service.GetRecyclingMachineAsync(CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No recycling machine found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetRecyclingMachineAsync_ShouldReturnNull_WhenApiReturnsNoMachines()
        {
            // Arrange
            var marketResponse = new MachineMarketResponse { machines = new List<MachineDto>() };
            var responseJson = JsonSerializer.Serialize(marketResponse);
            
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _service.GetRecyclingMachineAsync(CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No machines found in market response")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}