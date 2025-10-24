using System;
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
using Xunit;



namespace Recycler.Tests.Services
{
    public class BankAccountServiceTests
    {
        private readonly Mock<IHttpClientFactory> _clientFactoryMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<BankAccountService>> _loggerMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly BankAccountService _service;
        private const string BaseUrl = "http://bank-api.com/";
        private const string ExpectedAccountNum = "123456789";

        public BankAccountServiceTests()
        {
            _clientFactoryMock = new Mock<IHttpClientFactory>();
            _configMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<BankAccountService>>();
            _handlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_handlerMock.Object);

            _clientFactoryMock.Setup(f => f.CreateClient("test")).Returns(_httpClient);
            _configMock.SetupGet(c => c["commercialBankUrl"]).Returns(BaseUrl);

            _service = new BankAccountService(_clientFactoryMock.Object, _configMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnAccountNumber_OnSuccessfulInitialCreation()
        {
            // Arrange
            var accountResponseJson = JsonSerializer.Serialize(new { account_number = ExpectedAccountNum });
            
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.ToString().Contains("/api/account")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(accountResponseJson, System.Text.Encoding.UTF8, "application/json")
                })
                .Verifiable();

            // Act
            var result = await _service.RegisterAsync("http://notify.url", CancellationToken.None);

            // Assert
            result.Should().Be(ExpectedAccountNum);
            _handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            
            
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnAccountNumber_OnConflictAndSuccessfulGet()
        {
            // Arrange
            var accountResponseJson = JsonSerializer.Serialize(new { account_number = ExpectedAccountNum });
            
           
            _handlerMock
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict)) 
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) 
                {
                    Content = new StringContent(accountResponseJson, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _service.RegisterAsync("http://notify.url", CancellationToken.None);

            // Assert
            result.Should().Be(ExpectedAccountNum);
            
     
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>()
            );
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().Contains("api/account/me")),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        
        [Fact]
        public async Task RegisterAsync_ShouldReturnNull_WhenRegistrationFailsToReturnAccountNumber()
        {
            // Arrange
            var accountResponseJson = JsonSerializer.Serialize(new { some_other_field = "data" });
            
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(accountResponseJson, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _service.RegisterAsync("http://notify.url", CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no account number returned")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}