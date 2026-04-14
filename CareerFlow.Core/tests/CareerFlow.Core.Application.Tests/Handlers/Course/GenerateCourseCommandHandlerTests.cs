using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Application.CQRS.Courses.Handlers;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.AI.Dto;
using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Course;

public class GenerateCourseCommandHandlerTests
{
    private readonly Mock<ICourseService> _courseServiceMock = new();
    private readonly GenerateCourseCommandHandler _sut;

    public GenerateCourseCommandHandlerTests()
    {
        _sut = new GenerateCourseCommandHandler(_courseServiceMock.Object);
    }

    [Fact]
    public void Constructor_NullCourseService_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new GenerateCourseCommandHandler(null!));
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsGeneratedCourseId()
    {
        var command = new GenerateCourseCommand(Guid.NewGuid(), "C# Advanced");
        // Fixed: Using SkeletonDto and added EstimatedDays (e.g., 5)
        var skeletonResponse = new CourseSkeletonResponse(new SkeletonDto("C# Advanced", []), 5);
        var expectedCourseId = Guid.NewGuid();

        _courseServiceMock
            .Setup(s => s.GetCourseSkeletonAsync(It.IsAny<CourseSkeletonRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(skeletonResponse);
        _courseServiceMock
            .Setup(s => s.SaveCourseContentAsync(command.UserId, "C# Advanced", skeletonResponse,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCourseId);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldBe(expectedCourseId);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsGetCourseSkeletonWithCorrectTopic()
    {
        var command = new GenerateCourseCommand(Guid.NewGuid(), "Machine Learning");
        // Fixed: Using SkeletonDto and added EstimatedDays
        var skeletonResponse = new CourseSkeletonResponse(new SkeletonDto("Machine Learning", []), 5);

        _courseServiceMock
            .Setup(s => s.GetCourseSkeletonAsync(It.Is<CourseSkeletonRequest>(r => r.Topic == "Machine Learning"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(skeletonResponse);
        _courseServiceMock
            .Setup(s => s.SaveCourseContentAsync(It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<CourseSkeletonResponse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        await _sut.Handle(command, CancellationToken.None);

        _courseServiceMock.Verify(
            s => s.GetCourseSkeletonAsync(It.Is<CourseSkeletonRequest>(r => r.Topic == "Machine Learning"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsSaveCourseWithCorrectUserId()
    {
        var userId = Guid.NewGuid();
        var command = new GenerateCourseCommand(userId, "Topic");
        // Fixed: Using SkeletonDto and added EstimatedDays
        var skeletonResponse = new CourseSkeletonResponse(new SkeletonDto("Topic", []), 5);

        _courseServiceMock
            .Setup(s => s.GetCourseSkeletonAsync(It.IsAny<CourseSkeletonRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(skeletonResponse);
        _courseServiceMock
            .Setup(s => s.SaveCourseContentAsync(userId, "Topic", skeletonResponse, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        await _sut.Handle(command, CancellationToken.None);

        _courseServiceMock.Verify(
            s => s.SaveCourseContentAsync(userId, "Topic", skeletonResponse, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_GetSkeletonThrows_PropagatesException()
    {
        var command = new GenerateCourseCommand(Guid.NewGuid(), "Topic");
        _courseServiceMock
            .Setup(s => s.GetCourseSkeletonAsync(It.IsAny<CourseSkeletonRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("AI unavailable"));

        await Should.ThrowAsync<HttpRequestException>(() =>
            _sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SaveThrows_PropagatesException()
    {
        var command = new GenerateCourseCommand(Guid.NewGuid(), "Topic");
        // Fixed: Using SkeletonDto and added EstimatedDays
        var skeletonResponse = new CourseSkeletonResponse(new SkeletonDto("Topic", []), 5);

        _courseServiceMock
            .Setup(s => s.GetCourseSkeletonAsync(It.IsAny<CourseSkeletonRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(skeletonResponse);
        _courseServiceMock
            .Setup(s => s.SaveCourseContentAsync(It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<CourseSkeletonResponse>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Persistence error"));

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnedIdIsNotEmpty()
    {
        var command = new GenerateCourseCommand(Guid.NewGuid(), "Topic");
        // Fixed: Using SkeletonDto and added EstimatedDays
        var skeletonResponse = new CourseSkeletonResponse(new SkeletonDto("Topic", []), 5);

        _courseServiceMock
            .Setup(s => s.GetCourseSkeletonAsync(It.IsAny<CourseSkeletonRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(skeletonResponse);
        _courseServiceMock
            .Setup(s => s.SaveCourseContentAsync(It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<CourseSkeletonResponse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
    }
}