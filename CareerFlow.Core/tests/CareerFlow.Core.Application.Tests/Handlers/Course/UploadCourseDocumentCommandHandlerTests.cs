using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Application.CQRS.Courses.Handlers;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.Course.Dto;
using CareerFlow.Core.Domain.Models.Course.Response;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Course;

public class UploadCourseDocumentCommandHandlerTests
{
    private readonly Mock<ICourseService> _courseServiceMock = new();
    private readonly UploadCourseDocumentCommandHandler _sut;

    public UploadCourseDocumentCommandHandlerTests()
    {
        _sut = new UploadCourseDocumentCommandHandler(_courseServiceMock.Object);
    }

    private static List<UploadFileDto> CreateFiles(int count = 1)
    {
        return Enumerable.Range(0, count)
            .Select(i => new UploadFileDto($"test{i}.pdf", "application/pdf", new MemoryStream([1, 2, 3])))
            .ToList();
    }

    [Fact]
    public void Constructor_NullCourseService_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new UploadCourseDocumentCommandHandler(null!));
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUploadCoursesResponse()
    {
        var userId = Guid.NewGuid();
        var files = CreateFiles();
        var command = new UploadCourseDocumentCommand(userId, "My Course", files);
        var expected = new UploadCoursesResponse([], 0, 0, 0, []);

        _courseServiceMock
            .Setup(s => s.UploadManyAsync(userId, files, "My Course", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUploadManyOnce()
    {
        var userId = Guid.NewGuid();
        var files = CreateFiles();
        var command = new UploadCourseDocumentCommand(userId, "Title", files);

        _courseServiceMock
            .Setup(s => s.UploadManyAsync(userId, files, "Title", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UploadCoursesResponse([], 1, 1, 0, []));

        await _sut.Handle(command, CancellationToken.None);

        _courseServiceMock.Verify(
            s => s.UploadManyAsync(userId, files, "Title", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ServiceThrows_PropagatesException()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "Title", CreateFiles());

        _courseServiceMock
            .Setup(s => s.UploadManyAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<UploadFileDto>>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Storage error"));

        await Should.ThrowAsync<IOException>(() =>
            _sut.Handle(command, CancellationToken.None));
    }
}