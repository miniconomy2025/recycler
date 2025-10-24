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

    }
}
