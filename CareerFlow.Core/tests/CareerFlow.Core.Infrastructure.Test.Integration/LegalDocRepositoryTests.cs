using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Test.Integration.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Test.Integration;

[Trait("Category", "Integration")]
public class LegalDocRepositoryTests : BaseRepositoryTest
{
    private readonly LegalDocRepository _sut;

    public LegalDocRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new LegalDocRepository(Context.LegalDocs);
    }

    [Fact]
    public async Task GetLegalDocByTypeAsync_ExistingType_ReturnsDocument()
    {
        // Arrange
        var doc = LegalDoc.Create("termsOfService", "TermsAndConditions");
        Context.LegalDocs.Add(doc);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetLegalDocByTypeAsync("TermsAndConditions", CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Type.ShouldBe(LegalDocType.TermsAndConditions);
    }

    [Fact]
    public async Task GetLegalDocByTypeAsync_NonExistentType_ReturnsNull()
    {
        var result = await _sut.GetLegalDocByTypeAsync("PrivacyPolicy", CancellationToken.None);
        result.ShouldBeNull();
    }
}