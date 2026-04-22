using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Application.CQRS.Courses.Handlers;
using CareerFlow.Core.Domain.Abstractions.Services;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Course;

public class FinishChapterCommandHandlerTests
{
    private readonly Mock<ICourseService> _courseServiceMock = new();
    private readonly FinishChapterCommandHandler _sut;

    public FinishChapterCommandHandlerTests()
    {
        _sut = new FinishChapterCommandHandler(_courseServiceMock.Object);
    }

    [Fact]
    public void Constructor_NullCourseService_ThrowsArgumentNullException() => Should.Throw<ArgumentNullException>(() => new FinishChapterCommandHandler(null!));

    [Fact]
    public async Task Handle_ValidCommand_CallsFinishChapterAsyncOnce()
    {
        var command = new FinishChapterCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _courseServiceMock
            .Setup(s => s.FinishChapterAsync(command.UserId, command.CourseId, command.ChapterId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _courseServiceMock.Verify(
            s => s.FinishChapterAsync(command.UserId, command.CourseId, command.ChapterId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCorrectIds()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var chapterId = Guid.NewGuid();
        var command = new FinishChapterCommand(userId, courseId, chapterId);
        _courseServiceMock
            .Setup(s => s.FinishChapterAsync(userId, courseId, chapterId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _courseServiceMock.Verify(
            s => s.FinishChapterAsync(userId, courseId, chapterId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ServiceThrows_PropagatesException()
    {
        var command = new FinishChapterCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _courseServiceMock
            .Setup(s => s.FinishChapterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToService()
    {
        var command = new FinishChapterCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var cts = new CancellationTokenSource();
        _courseServiceMock
            .Setup(s => s.FinishChapterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), cts.Token))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, cts.Token);

        _courseServiceMock.Verify(
            s => s.FinishChapterAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), cts.Token),
            Times.Once);
    }
}
