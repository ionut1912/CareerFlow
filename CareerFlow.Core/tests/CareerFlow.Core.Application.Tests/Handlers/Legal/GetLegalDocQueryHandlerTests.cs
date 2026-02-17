using CareerFlow.Core.Application.CQRS.Legal.Handler;
using CareerFlow.Core.Application.CQRS.Legal.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Legal;

public class GetLegalDocQueryHandlerTests : BaseHandlerTest<GetLegalDocQueryHandler>
{
    private readonly Mock<ILegalDocRepository> _legalDocRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetLegalDocQueryHandler _handler;

    public GetLegalDocQueryHandlerTests()
    {
        _legalDocRepositoryMock = new Mock<ILegalDocRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetLegalDocQueryHandler(
            _legalDocRepositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValueInCache_ReturnsFromCache()
    {
        // Arrange
        var dto = new LegalDocDto("content", "PrivacyPolicy");
        var query = new GetLegalDocQuery(dto.Type);
        _cacheServiceMock.Setup(x => x.GetCacheValueAsync<LegalDocDto>($"LegalDoc_{query.Type}")).ReturnsAsync(dto);

        // Act
        var result = await _handler.Handle(query, Ct);

        // Assert
        result.Content.ShouldBe(dto.Content);
        _legalDocRepositoryMock.Verify(x => x.GetLegalDocByTypeAsync(It.IsAny<string>(), Ct), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValueNotInCache_ReturnsFromDbAndSetsCache()
    {
        // Arrange
        var doc = LegalDoc.Create("content", "PrivacyPolicy");
        var query = new GetLegalDocQuery(doc.Type.Value);

        _cacheServiceMock.Setup(x => x.GetCacheValueAsync<LegalDocDto>(It.IsAny<string>())).ReturnsAsync((LegalDocDto?)null);
        _legalDocRepositoryMock.Setup(x => x.GetLegalDocByTypeAsync(query.Type, Ct)).ReturnsAsync(doc);

        // Act
        var result = await _handler.Handle(query, Ct);

        // Assert
        result.Content.ShouldBe(doc.Content);
        _cacheServiceMock.Verify(x => x.SetCacheValueAsync(It.IsAny<string>(), It.IsAny<LegalDocDto>()), Times.Once);
    }
}