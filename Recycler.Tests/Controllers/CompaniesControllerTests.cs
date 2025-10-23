using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Recycler.API.Commands;
using Recycler.API.Controllers;
using Recycler.API.Models;
using Recycler.API.Services;
using Xunit;

namespace Recycler.Tests.Controllers
{
    public class CompaniesControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogService> _logServiceMock;
        private readonly CompaniesController _controller;

        public CompaniesControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _logServiceMock = new Mock<ILogService>();

            _controller = new CompaniesController(_mediatorMock.Object, _logServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task CreateCompany_ShouldReturn201Created_WhenSuccessful()
        {
            // Arrange
            var command = new CreateCompanyCommand
            {
                Name = "EcoRecycler",
                KeyId = 2
            };

            var expectedResponse = new CreateCompanyResponse
            {
                CompanyId = 101,
                CompanyNumber = Guid.NewGuid(),
                Name = "EcoRecycler",
                Role = "Supplier"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateCompany(command);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(CompaniesController.CreateCompany));
            createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
            createdResult.Value.Should().BeEquivalentTo(expectedResponse);

            _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
            _logServiceMock.Verify(
                l => l.CreateLog(It.IsAny<HttpContext>(), command, It.IsAny<IActionResult>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateCompany_ShouldIncludeCorrectRouteValues()
        {
            // Arrange
            var command = new CreateCompanyCommand { Name = "GreenTech" };
            var response = new CreateCompanyResponse
            {
                CompanyId = 202,
                CompanyNumber = Guid.NewGuid(),
                Name = "GreenTech",
                Role = "Recycler"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateCompany(command);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.RouteValues.Should().ContainKey("id");
            createdResult.RouteValues["id"].Should().Be(response.CompanyId);
        }

        [Fact]
        public async Task CreateCompany_ShouldStillThrow_WhenMediatorThrowsException()
        {
            // Arrange
            var command = new CreateCompanyCommand { Name = "FailCo" };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateCompanyCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            Func<Task> act = async () => await _controller.CreateCompany(command);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Database connection failed");

            _logServiceMock.Verify(
                l => l.CreateLog(It.IsAny<HttpContext>(), command, It.IsAny<IActionResult>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateCompany_ShouldCreateLog_WithSameResponseInstance()
        {
            // Arrange
            var command = new CreateCompanyCommand { Name = "EcoFriendly" };
            var response = new CreateCompanyResponse
            {
                CompanyId = 303,
                CompanyNumber = Guid.NewGuid(),
                Name = "EcoFriendly",
                Role = "Supplier"
            };

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            object? loggedResponse = null;

            _logServiceMock
                .Setup(l => l.CreateLog(It.IsAny<HttpContext>(), It.IsAny<object>(), It.IsAny<object>()))
                .Callback<HttpContext, object, object>((ctx, cmd, result) => loggedResponse = result)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateCompany(command);

            // Assert
            loggedResponse.Should().NotBeNull();
            loggedResponse.Should().BeEquivalentTo(result);

            _logServiceMock.Verify(
                l => l.CreateLog(It.IsAny<HttpContext>(), command, It.IsAny<object>()),
                Times.Once);
        }
    }
}
