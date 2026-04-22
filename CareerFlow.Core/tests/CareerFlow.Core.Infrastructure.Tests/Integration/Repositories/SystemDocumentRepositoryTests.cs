using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistence.Repositories;
using CareerFlow.Core.Infrastructure.Tests.Integration.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
[Collection("RepositoryCollection")]
public class SystemDocumentRepositoryTests : BaseRepositoryTest, IAsyncLifetime
{
    private readonly SystemDocumentRepository _sut;

    public SystemDocumentRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new SystemDocumentRepository(Context.SystemDocuments);
    }

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Context.SystemDocuments.RemoveRange(Context.SystemDocuments);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    // ---------------------------------------------------------------------------
    // AddAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_SystemDocument_PersistsToDatabase()
    {
        // Arrange
        var document = SystemDocument.Create("Terms", "\"etag123\"");

        // Act
        await _sut.AddAsync(document, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.SystemDocuments.FindAsync(document.Id);
        saved.ShouldNotBeNull();
        saved!.DocumentType.ShouldBe("Terms");
        saved.CurrentETag.ShouldBe("\"etag123\"");
    }

    // ---------------------------------------------------------------------------
    // GetByIdAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsDocument()
    {
        // Arrange
        SystemDocument document = await SeedDocumentAsync("Terms", "\"etag1\"");

        // Act
        var result = await _sut.GetByIdAsync(document.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(document.Id);
        result.DocumentType.ShouldBe("Terms");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    // ---------------------------------------------------------------------------
    // GetAllAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_MultipleDocuments_ReturnsAll()
    {
        // Arrange
        await SeedDocumentAsync("Terms", "\"etag1\"");
        await SeedDocumentAsync("Privacy", "\"etag2\"");

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(d => d.DocumentType == "Terms");
        result.ShouldContain(d => d.DocumentType == "Privacy");
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsEmptyCollection()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.ShouldBeEmpty();
    }

    // ---------------------------------------------------------------------------
    // Delete
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Delete_ExistingDocument_RemovesFromDatabase()
    {
        // Arrange
        var document = await SeedDocumentAsync("Terms", "\"etag1\"");
        var tracked = await Context.SystemDocuments.FindAsync(document.Id);

        // Act
        _sut.Delete(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var deleted = await Context.SystemDocuments.FindAsync(document.Id);
        deleted.ShouldBeNull();
    }

    // ---------------------------------------------------------------------------
    // Update
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Update_ChangeETag_IsPersisted()
    {
        // Arrange
        var document = await SeedDocumentAsync("Terms", "\"old\"");
        var tracked = await Context.SystemDocuments.FindAsync(document.Id);
        tracked!.Update("\"new\"");

        // Act
        _sut.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.SystemDocuments.FindAsync(document.Id);
        refreshed!.CurrentETag.ShouldBe("\"new\"");
    }

    // ---------------------------------------------------------------------------
    // FindByTypeAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task FindByTypeAsync_ExistingType_ReturnsDocument()
    {
        // Arrange
        await SeedDocumentAsync("Terms", "\"etag1\"");

        // Act
        var result = await _sut.FindByTypeAsync("Terms", CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.DocumentType.ShouldBe("Terms");
        result.CurrentETag.ShouldBe("\"etag1\"");
    }

    [Fact]
    public async Task FindByTypeAsync_NonExistentType_ReturnsNull()
    {
        // Act
        var result = await _sut.FindByTypeAsync("NonExistent", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task FindByTypeAsync_MultipleDocuments_ReturnsCorrectOne()
    {
        // Arrange
        await SeedDocumentAsync("Terms", "\"etag1\"");
        await SeedDocumentAsync("Privacy", "\"etag2\"");

        // Act
        var result = await _sut.FindByTypeAsync("Privacy", CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.DocumentType.ShouldBe("Privacy");
        result.CurrentETag.ShouldBe("\"etag2\"");
    }

    [Fact]
    public async Task FindByTypeAsync_TypeIsCaseSensitive_ReturnsNull()
    {
        // Arrange
        await SeedDocumentAsync("Terms", "\"etag1\"");

        // Act
        var result = await _sut.FindByTypeAsync("TERMS", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    // ---------------------------------------------------------------------------
    // Helper
    // ---------------------------------------------------------------------------

    private async Task<SystemDocument> SeedDocumentAsync(string type, string etag)
    {
        var document = SystemDocument.Create(type, etag);
        Context.SystemDocuments.Add(document);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return document;
    }
}
