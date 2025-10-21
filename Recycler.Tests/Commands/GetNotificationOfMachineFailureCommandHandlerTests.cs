using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Recycler.API;
using Recycler.API.Models;
using RecyclerApi.Commands;
using RecyclerApi.Handlers;
using Xunit;

namespace Recycler.Tests.Commands
{
    public class GetNotificationOfMachineFailureCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldMarkFirstOperationalMachineAsFailed_WhenMachineNameMatches()
        {
            // Arrange
            var machines = new List<ReceivedMachineDto>
            {
                new() { Id = 1, MachineId = 100, IsOperational = true },
                new() { Id = 2, MachineId = 101, IsOperational = true }
            };

            var mockRepo = new Mock<IGenericRepository<ReceivedMachineDto>>();
            mockRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(machines);

            var handler = new GetNotificationOfMachineFailureCommandHandler(mockRepo.Object);

            var command = new GetNotificationOfMachineFailureCommand
            {
                MachineName = "recycling_machine"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            machines[0].IsOperational.Should().BeFalse("the first operational machine should be marked as failed");
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<ReceivedMachineDto>(), It.Is<IEnumerable<string>>(props => props.Contains("IsOperational"))), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenNoMachinesAreOperational()
        {
            // Arrange
            var machines = new List<ReceivedMachineDto>
            {
                new() { Id = 1, MachineId = 100, IsOperational = false },
                new() { Id = 2, MachineId = 101, IsOperational = false }
            };

            var mockRepo = new Mock<IGenericRepository<ReceivedMachineDto>>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(machines);

            var handler = new GetNotificationOfMachineFailureCommandHandler(mockRepo.Object);
            var command = new GetNotificationOfMachineFailureCommand { MachineName = "recycling_machine" };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<ReceivedMachineDto>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenMachineNameDoesNotMatch()
        {
            // Arrange
            var machines = new List<ReceivedMachineDto>
            {
                new() { Id = 1, MachineId = 100, IsOperational = true }
            };

            var mockRepo = new Mock<IGenericRepository<ReceivedMachineDto>>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(machines);

            var handler = new GetNotificationOfMachineFailureCommandHandler(mockRepo.Object);
            var command = new GetNotificationOfMachineFailureCommand
            {
                MachineName = "other_machine"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            machines[0].IsOperational.Should().BeTrue("no machine should change when the name doesn't match");
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<ReceivedMachineDto>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenNoMachinesExist()
        {
            // Arrange
            var mockRepo = new Mock<IGenericRepository<ReceivedMachineDto>>();
            mockRepo.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(new List<ReceivedMachineDto>()); // empty list

            var handler = new GetNotificationOfMachineFailureCommandHandler(mockRepo.Object);

            var command = new GetNotificationOfMachineFailureCommand
            {
                MachineName = "recycling_machine"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync("the handler should handle empty machine lists gracefully");
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<ReceivedMachineDto>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

    }
}
