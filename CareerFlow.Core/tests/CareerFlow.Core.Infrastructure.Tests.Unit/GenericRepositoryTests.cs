using Microsoft.EntityFrameworkCore;
using Shared.Domain.Common;
using Shared.Infra.Services;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit;

public class GenericRepositoryTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly GenericRepository<TestEntity> _sut;

    public GenericRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _sut = new GenericRepository<TestEntity>(_context.Entities);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void Constructor_NullDbSet_ThrowsArgumentNullException()
    {
        // Assert
        Should.Throw<ArgumentNullException>(() => new GenericRepository<TestEntity>(null!));
    }

    [Fact]
    public async Task AddAsync_ValidEntity_EntityIsPersisted()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test" };

        // Act
        await _sut.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Assert
        var found = await _context.Entities.FindAsync(entity.Id);
        found.ShouldNotBeNull();
        found.Name.ShouldBe("Test");
    }

    [Fact]
    public async Task AddAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var entity = new TestEntity { Name = "Token Test" };
        using var cts = new CancellationTokenSource();

        // Act
        await _sut.AddAsync(entity, cts.Token);
        await _context.SaveChangesAsync();

        // Assert
        var found = await _context.Entities.FindAsync(entity.Id);
        found.ShouldNotBeNull();
    }

    [Fact]
    public async Task Delete_ExistingEntity_EntityIsRemovedAfterSave()
    {
        // Arrange
        var entity = new TestEntity { Name = "To Delete" };
        _context.Entities.Add(entity);
        await _context.SaveChangesAsync();

        // Act
        _sut.Delete(entity);
        await _context.SaveChangesAsync();

        // Assert
        var found = await _context.Entities.FindAsync(entity.Id);
        found.ShouldBeNull();
    }

    [Fact]
    public async Task Update_ExistingEntity_ChangesArePersistedAfterSave()
    {
        // Arrange
        var entity = new TestEntity {  Name = "Original" };
        _context.Entities.Add(entity);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var loaded = (await _context.Entities.FindAsync(entity.Id))!;
        loaded.Name = "Updated";

        // Act
        _sut.Update(loaded);
        await _context.SaveChangesAsync();

        // Assert
        _context.ChangeTracker.Clear();
        var updated = await _context.Entities.FindAsync(entity.Id);
        updated!.Name.ShouldBe("Updated");
    }

    [Fact]
    public async Task GetAllAsync_NoIncludes_ReturnsAllEntities()
    {
        // Arrange
        _context.Entities.AddRange(
            new TestEntity {  Name = "A" },
            new TestEntity { Name = "B" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsEmptyCollection()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_NullIncludes_ReturnsAllEntitiesWithoutThrowing()
    {
        // Arrange
        _context.Entities.Add(new TestEntity { Name = "X" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync(CancellationToken.None, null!);

        // Assert
        result.Count().ShouldBe(1);
    }

    public class TestEntity:Entity
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities => Set<TestEntity>();
    }
}