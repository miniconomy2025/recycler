using Xunit;
using Moq;
using FluentAssertions;
using Recycler.API.Commands.StartSimulation;
using Recycler.API.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Recycler.Tests.Commands
{
    public class StartSimulationCommandHandlerTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ISimulationClock> _clockMock;
        private readonly Mock<IDatabaseResetService> _resetServiceMock;
        private readonly Mock<ISimulationBootstrapService> _bootstrapMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<IServiceProvider> _providerMock;

        public StartSimulationCommandHandlerTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                                  .Returns(new HttpClient());

            _clockMock = new Mock<ISimulationClock>();
            _resetServiceMock = new Mock<IDatabaseResetService>();
            _bootstrapMock = new Mock<ISimulationBootstrapService>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();
            _providerMock = new Mock<IServiceProvider>();

            _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);
            _scopeMock.SetupGet(s => s.ServiceProvider).Returns(_providerMock.Object);
            _providerMock.Setup(p => p.GetService(typeof(ISimulationBootstrapService)))
                         .Returns(_bootstrapMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldResetDatabase_AndStartClock_AndReturnStartedStatus()
        {
            _bootstrapMock.Setup(b => b.RunAsync(It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

            var handler = new StartSimulationCommandHandler(
                _httpClientFactoryMock.Object,
                _clockMock.Object,
                _resetServiceMock.Object,
                _scopeFactoryMock.Object
            );

            var command = new StartSimulationCommand
            {
                EpochStartTime = null
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("started");
            result.Message.Should().Contain("Simulation clock started at");

            _resetServiceMock.Verify(r => r.ResetAsync(It.IsAny<CancellationToken>()), Times.Once);
            _clockMock.Verify(c => c.Start(It.IsAny<DateTime?>()), Times.Once);
            _bootstrapMock.Verify(b => b.RunAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldStartClock_WithSpecificEpochTime_WhenProvided()
        {
            // Arrange
            _bootstrapMock.Setup(b => b.RunAsync(It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

            var handler = new StartSimulationCommandHandler(
                _httpClientFactoryMock.Object,
                _clockMock.Object,
                _resetServiceMock.Object,
                _scopeFactoryMock.Object
            );

            var epoch = new DateTimeOffset(new DateTime(2050, 1, 1, 0, 0, 0, DateTimeKind.Utc)).ToUnixTimeMilliseconds();
            var command = new StartSimulationCommand { EpochStartTime = epoch };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be("started");
            _clockMock.Verify(c => c.Start(It.Is<DateTime?>(d => d.HasValue && d.Value.Year == 2050)), Times.Once);
            _resetServiceMock.Verify(r => r.ResetAsync(It.IsAny<CancellationToken>()), Times.Once);
            _bootstrapMock.Verify(b => b.RunAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
