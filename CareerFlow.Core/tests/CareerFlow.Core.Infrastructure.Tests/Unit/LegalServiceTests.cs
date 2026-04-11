using System.Net;
using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Infrastructure.Services;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit;

public class LegalServiceTests
{
    private readonly Mock<IGithubPagesRequestsSender> _requestsSenderMock;
    private readonly LegalService _sut;

    public LegalServiceTests()
    {
        _requestsSenderMock = new Mock<IGithubPagesRequestsSender>();
        _sut = new LegalService(_requestsSenderMock.Object);
    }

    [Fact]
    public async Task GetDocumentAsync_WhenResponseIsSuccess_ShouldReturnLegalDocument()
    {
        // Arrange
        var docType = "terms-of-service";
        var content = "Standard Legal Text";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        };

        _requestsSenderMock
            .Setup(x => x.GetContentAsync(docType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.GetDocumentAsync(docType, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Content.ShouldBe(content);
        result.Source.ShouldBe("GitHub Pages");
    }

    [Fact]
    public async Task GetDocumentAsync_WhenResponseFails_ShouldReturnNull()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _requestsSenderMock
            .Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.GetDocumentAsync("any-type", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WhenSenderIsNull_ShouldThrowException()
    {
        // Act & Assert (Sintaxa curată Shouldly pentru excepții)
        Should.Throw<ArgumentNullException>(() => new LegalService(null!));
    }
}