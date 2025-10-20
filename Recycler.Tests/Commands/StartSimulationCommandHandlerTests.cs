using Xunit;
using Moq;
using FluentAssertions;
using Recycler.API.Commands.StartSimulation;
using Recycler.API.Services;


namespace Recycler.Tests.Commands
{
    public class StartSimulationCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldResetDatabase_AndStartClock_AndReturnStartedStatus()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                                 .Returns(new HttpClient());

            var clockMock = new Mock<ISimulationClock>();
            var resetServiceMock = new Mock<IDatabaseResetService>();
            var bootstrapMock = new Mock<ISimulationBootstrapService>();

            bootstrapMock.Setup(b => b.RunAsync(It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

            var handler = new StartSimulationCommandHandler(
                httpClientFactoryMock.Object,
                clockMock.Object,
                resetServiceMock.Object,
                bootstrapMock.Object
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

            // Verify expected calls
            resetServiceMock.Verify(r => r.ResetAsync(It.IsAny<CancellationToken>()), Times.Once);
            clockMock.Verify(c => c.Start(It.IsAny<DateTime?>()), Times.Once);
            bootstrapMock.Verify(b => b.RunAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldStartClock_WithSpecificEpochTime_WhenProvided()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                                 .Returns(new HttpClient());

            var clockMock = new Mock<ISimulationClock>();
            var resetServiceMock = new Mock<IDatabaseResetService>();
            var bootstrapMock = new Mock<ISimulationBootstrapService>();

            bootstrapMock.Setup(b => b.RunAsync(It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

            var handler = new StartSimulationCommandHandler(
                httpClientFactoryMock.Object,
                clockMock.Object,
                resetServiceMock.Object,
                bootstrapMock.Object
            );

            var epoch = new DateTimeOffset(new DateTime(2050, 1, 1, 0, 0, 0, DateTimeKind.Utc)).ToUnixTimeMilliseconds();
            var command = new StartSimulationCommand { EpochStartTime = epoch };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be("started");
            clockMock.Verify(c => c.Start(It.Is<DateTime?>(d => d.HasValue && d.Value.Year == 2050)), Times.Once);
            resetServiceMock.Verify(r => r.ResetAsync(It.IsAny<CancellationToken>()), Times.Once);
            bootstrapMock.Verify(b => b.RunAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
