using FluentAssertions;
using Recycler.API.Commands;
using Recycler.API.Handlers;

namespace Recycler.Tests.Commands
{
    public class CreateCompanyCommandHandlerTests
    {
        private readonly CreateCompanyCommandHandler _handler;

        public CreateCompanyCommandHandlerTests()
        {
            _handler = new CreateCompanyCommandHandler();
        }

        [Theory]
        [InlineData(1, "Recycler")]
        [InlineData(2, "Supplier")]
        [InlineData(3, "Logistics")]
        [InlineData(4, "Bank")]
        [InlineData(99, "General")]
        public async Task Handle_ShouldAssignCorrectRole_BasedOnKeyId(int keyId, string expectedRole)
        {
            // Arrange
            var command = new CreateCompanyCommand
            {
                KeyId = keyId,
                Name = $"TestCompany{keyId}"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Role.Should().Be(expectedRole);
            result.Name.Should().Be(command.Name);
            result.CompanyId.Should().BeGreaterThan(0);
            result.CompanyNumber.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task Handle_ShouldIncrementCompanyIdEachTime()
        {
            // Arrange
            var firstCommand = new CreateCompanyCommand { KeyId = 1, Name = "FirstCo" };
            var secondCommand = new CreateCompanyCommand { KeyId = 2, Name = "SecondCo" };

            // Act
            var firstResult = await _handler.Handle(firstCommand, CancellationToken.None);
            var secondResult = await _handler.Handle(secondCommand, CancellationToken.None);

            // Assert
            secondResult.CompanyId.Should().BeGreaterThan(firstResult.CompanyId);
        }

        [Fact]
        public async Task Handle_ShouldGenerateUniqueCompanyNumbers()
        {
            // Arrange
            var command1 = new CreateCompanyCommand { KeyId = 1, Name = "Alpha" };
            var command2 = new CreateCompanyCommand { KeyId = 1, Name = "Beta" };

            // Act
            var result1 = await _handler.Handle(command1, CancellationToken.None);
            var result2 = await _handler.Handle(command2, CancellationToken.None);

            // Assert
            result1.CompanyNumber.Should().NotBe(result2.CompanyNumber);
        }

        [Fact]
        public async Task Handle_ShouldAssignGeneralRole_WhenKeyIdIsNegative()
        {
            // Arrange
            var command = new CreateCompanyCommand
            {
                KeyId = -5,
                Name = "NegativeKeyTest"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Role.Should().Be("General");
            result.Name.Should().Be("NegativeKeyTest");
        }

        [Fact]
        public async Task Handle_ShouldStillReturnResponse_WhenNameIsEmpty()
        {
            // Arrange
            var command = new CreateCompanyCommand
            {
                KeyId = 1,
                Name = string.Empty
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Role.Should().Be("Recycler");
            result.Name.Should().BeEmpty(); 
            result.CompanyNumber.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task Handle_ShouldNotThrow_WhenCalledConcurrently()
        {
            // Arrange
            var tasks = Enumerable.Range(1, 5)
                .Select(i => _handler.Handle(new CreateCompanyCommand
                {
                    KeyId = i,
                    Name = $"Concurrent_{i}"
                }, CancellationToken.None));

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().OnlyHaveUniqueItems(r => r.CompanyId);
            results.Should().OnlyHaveUniqueItems(r => r.CompanyNumber);
        }

    }
}
