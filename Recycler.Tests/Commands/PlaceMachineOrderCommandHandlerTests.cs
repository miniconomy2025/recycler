using Xunit;
using Moq;
using Moq.Protected;
using FluentAssertions;
using Recycler.API.Commands;
using Recycler.API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Recycler.Tests.Commands
{
    public class PlaceMachineOrderCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenThoHRespondsWithOk()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var responseDto = new MachineOrderResponseDto
            {
                Message = "Machine order placed successfully."
            };

            var jsonResponse = JsonSerializer.Serialize(responseDto);
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "thoHApiUrl", "http://fake-thoh-api.com" }
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var loggerMock = new Mock<ILogger<PlaceMachineOrderCommandHandler>>();
            var handler = new PlaceMachineOrderCommandHandler(httpClientFactoryMock.Object, config, loggerMock.Object);

            var command = new PlaceMachineOrderCommand
            {
                machineName = "RecyclerBot",
                quantity = 2
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be("Machine order placed successfully.");

            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri != null &&
                    req.RequestUri.AbsolutePath == "/machines"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task Handle_ShouldThrowApplicationException_WhenThoHRespondsWithError()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"status\":\"error\",\"message\":\"Invalid order\"}", Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "thoHApiUrl", "http://fake-thoh-api.com" }
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var loggerMock = new Mock<ILogger<PlaceMachineOrderCommandHandler>>();
            var handler = new PlaceMachineOrderCommandHandler(httpClientFactoryMock.Object, config, loggerMock.Object);

            var command = new PlaceMachineOrderCommand
            {
                machineName = "RecyclerBot",
                quantity = 5
            };

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                     .ThrowAsync<ApplicationException>()
                     .WithMessage("*Error communicating with ThoH API*");
        }

        [Fact]
        public async Task Handle_ShouldReturnDefaultMessage_WhenThoHResponseBodyIsEmpty()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("", Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "thoHApiUrl", "http://fake-thoh-api.com" }
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var loggerMock = new Mock<ILogger<PlaceMachineOrderCommandHandler>>();
            var handler = new PlaceMachineOrderCommandHandler(httpClientFactoryMock.Object, config, loggerMock.Object);

            var command = new API.Commands.PlaceMachineOrderCommand
            {
                machineName = "RecyclerBot",
                quantity = 1
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be("Machine order placed successfully."); 
        }
    }
}
