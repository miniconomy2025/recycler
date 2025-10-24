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
using Recycler.API.Models;
using Recycler.API.Dto;
using Recycler.API.Services;
using Recycler.API.Utils;
using Xunit;



namespace Recycler.Tests.Services
{
    public class MakePaymentServiceTests
    {
        private readonly Mock<IHttpClientFactory> _clientFactoryMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<CommercialBankService> _bankServiceMock;
        private readonly Mock<ISimulationClock> _clockMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly MakePaymentService _service;
        private const string BaseUrl = "http://bank-api.com/";
        private const string ExpectedTransactionNum = "T12345";

        public MakePaymentServiceTests()
        {
            _clientFactoryMock = new Mock<IHttpClientFactory>();
            _configMock = new Mock<IConfiguration>();
            _bankServiceMock = new Mock<CommercialBankService>();
            _clockMock = new Mock<ISimulationClock>();
            _handlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_handlerMock.Object);

            _clientFactoryMock.Setup(f => f.CreateClient("test")).Returns(_httpClient);
            _configMock.SetupGet(c => c["commercialBankUrl"]).Returns(BaseUrl);
            _clockMock.Setup(c => c.GetCurrentSimulationTime()).Returns(new DateTime(2025, 1, 1));

            _service = new MakePaymentService(
                _clientFactoryMock.Object, 
                _configMock.Object, 
                _bankServiceMock.Object, 
                _clockMock.Object);
        }

        [Fact]
        public async Task SendPaymentAsync_ShouldReturnPaymentResult_OnSuccess()
        {
            // Arrange
            const string accountNumber = "987654321";
            const decimal amount = 100.50m;
            const string description = "Recycling Payout";
            
            var expectedResult = new MakePaymentService.PaymentResult
            {
                success = true,
                transaction_number = ExpectedTransactionNum,
                status = "COMPLETED"
            };
            var expectedResponseJson = JsonSerializer.Serialize(expectedResult);
            
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.Method == HttpMethod.Post && 
                        req.RequestUri!.ToString().Contains("/api/transaction")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedResponseJson, System.Text.Encoding.UTF8, "application/json")
                })
                .Verifiable();

            // Act
            var result = await _service.SendPaymentAsync(accountNumber, amount, description);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            _handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            _clockMock.Verify(c => c.GetCurrentSimulationTime(), Times.Once);
        }

        [Fact]
        public async Task SendPaymentAsync_ShouldThrowApplicationException_OnFailureStatusCode()
        {
            // Arrange
            const string accountNumber = "987654321";
            const decimal amount = 100.50m;
            const string description = "Recycling Payout";
            const string errorBody = "Insufficient Funds";

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Forbidden) // 403 Forbidden
                {
                    Content = new StringContent(errorBody)
                });

            // Act & Assert
            await _service
                .Invoking(s => s.SendPaymentAsync(accountNumber, amount, description))
                .Should().ThrowAsync<ApplicationException>()
                .WithMessage("*Payment failed: Forbidden - Insufficient Funds*");
        }
    }
}