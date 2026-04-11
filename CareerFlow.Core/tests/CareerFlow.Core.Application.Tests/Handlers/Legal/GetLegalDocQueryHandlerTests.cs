using CareerFlow.Core.Application.CQRS.Legal.Handlers;
using CareerFlow.Core.Application.CQRS.Legal.Queries;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.Legal;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Legal;

public class GetLegalDocQueryHandlerTests : BaseHandlerTest<GetLegalDocQueryHandler>
{
    private readonly GetLegalDocQueryHandler _handler;
    private readonly Mock<ILegalService> _legalServiceMock;

    public GetLegalDocQueryHandlerTests()
    {
        _legalServiceMock = new Mock<ILegalService>();
        _handler = new GetLegalDocQueryHandler(_legalServiceMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("terms")]
    [InlineData("privacy")]
    [InlineData("Terms")]
    public async Task Handle_ValidType_ReturnsLegalDocument(string type)
    {
        // Arrange
        var query = new GetLegalDocQuery(type);
        var document = new LegalDocumentResponse("TestContent", "Github", DateTime.UtcNow);
        _legalServiceMock.Setup(x => x.GetDocumentAsync(query.Type, Ct))
            .ReturnsAsync(document);

        // Act
        var response = await _handler.Handle(query, Ct);

        // Assert
        response.ShouldNotBeNull();
        response.Content.ShouldBe(document.Content);
        response.Source.ShouldBe(document.Source);
        _legalServiceMock.Verify(x => x.GetDocumentAsync(query.Type, Ct), Times.Once);
    }

    [Theory]
    [InlineData("test")]
    [InlineData("Privaccy")]
    public async Task Handle_InValidType_ThrowsException(string type)
    {
        // Arrange
        var query = new GetLegalDocQuery(type);

        // Act
        var exception =
            await Should.ThrowAsync<LegalDocInvalidTypeException>(async () => await _handler.Handle(query, Ct));

        // Assert
        exception.Message.ShouldBe("Tipul precizat nu exista");
        _legalServiceMock.Verify(x => x.GetDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _loggerMock.VerifyLogError("Tipul precizat nu exista", Times.Once());
    }

    [Fact]
    public async Task Handle_DocumentNotFound_ThrowsException()
    {
        // Arrange
        var query = new GetLegalDocQuery("privacy");
        _legalServiceMock.Setup(x => x.GetDocumentAsync(query.Type, Ct))
            .ReturnsAsync((LegalDocumentResponse?)null);

        // Act
        var exception =
            await Should.ThrowAsync<LegalDocNotFoundException>(async () => await _handler.Handle(query, Ct));

        // Assert
        exception.Message.ShouldBe("Documentul nu a fost gasit");
        _legalServiceMock.Verify(x => x.GetDocumentAsync(query.Type, Ct), Times.Once);
        _loggerMock.VerifyLogError("Documentul nu a fost gasit", Times.Once());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Handle_WhenDependenciesAreNull_ThrowsArgumentNullException(bool isServiceNull, bool isLoggerNull)
    {
        // Arrange
        var query = new GetLegalDocQuery("privacy");
        
        // Use the null-forgiving operator (!) to satisfy the compiler while testing null guards
        var service = isServiceNull ? null! : _legalServiceMock.Object;
        var logger = isLoggerNull ? null! : _loggerMock.Object;

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => 
        {
            var handler = new GetLegalDocQueryHandler(service, logger);
            await handler.Handle(query, Ct);
        });
    }
}