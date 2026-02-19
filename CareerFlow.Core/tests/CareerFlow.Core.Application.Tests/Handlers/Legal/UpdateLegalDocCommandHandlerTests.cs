using CareerFlow.Core.Application.CQRS.Legal.Command;
using CareerFlow.Core.Application.CQRS.Legal.Handler;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Legal;

public class UpdateLegalDocCommandHandlerTests : BaseHandlerTest<UpdateLegalDocCommandHandler>
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly UpdateLegalDocCommandHandler _handler;
    private readonly Mock<ILegalDocRepository> _legalDocRepositoryMock;

    public UpdateLegalDocCommandHandlerTests()
    {
        _legalDocRepositoryMock = new Mock<ILegalDocRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new UpdateLegalDocCommandHandler(
            _legalDocRepositoryMock.Object,
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenDocExists_UpdatesDbAndCache()
    {
        // Arrange
        var doc = LegalDoc.Create("oldContent", "PrivacyPolicy");
        var command = new UpdateLegalDocCommand("newContent", "PrivacyPolicy");

        _legalDocRepositoryMock.Setup(x => x.GetLegalDocByTypeAsync(command.Type, Ct)).ReturnsAsync(doc);

        // Act
        var result = await _handler.Handle(command, Ct);

        // Assert
        result.Item1.Content.ShouldBe(command.Content);
        result.Item1.Type.ShouldBe(command.Type);
        _legalDocRepositoryMock.Verify(x => x.Update(doc), Times.Once);
        _unitOfWorkMock.VerifySaveChanges(Times.Once());
        _cacheServiceMock.Verify(x => x.SetCacheValueAsync(It.IsAny<string>(), It.IsAny<LegalDocDto>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDocDoesNotExist_ThrowsException()
    {
        // Arrange
        var command = new UpdateLegalDocCommand("content", "UnknownType");
        _legalDocRepositoryMock.Setup(x => x.GetLegalDocByTypeAsync(command.Type, Ct)).ReturnsAsync((LegalDoc?)null);

        // Act
        await Should.ThrowAsync<LegalDocNotFoundException>(() => _handler.Handle(command, Ct));

        // Assert
        _loggerMock.VerifyLogError(command.Type, Times.Once());
        _unitOfWorkMock.VerifySaveChanges(Times.Never());
    }
}