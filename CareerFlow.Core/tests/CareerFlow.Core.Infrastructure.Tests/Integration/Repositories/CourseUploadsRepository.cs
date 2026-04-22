using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistence.Repositories;
using CareerFlow.Core.Infrastructure.Tests.Integration.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class CourseUploadsRepositoryTests : BaseRepositoryTest, IAsyncLifetime
{
    private readonly CourseUploadsRepository _sut;

    public CourseUploadsRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new CourseUploadsRepository(Context.CourseUploads);
    }

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Context.CourseJobs.RemoveRange(Context.CourseJobs);
        Context.CourseUploads.RemoveRange(Context.CourseUploads);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_Upload_PersistsToDatabase()
    {
        // Arrange
        var upload = CreateUpload();

        // Act
        await _sut.AddAsync(upload, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.CourseUploads.FindAsync(upload.Id);
        saved.ShouldNotBeNull();
        saved!.Title.ShouldBe("Test Title");
        saved.FileName.ShouldBe("file.pdf");
        saved.FileKey.ShouldBe("file-key");
        saved.FileType.ShouldBe("application/pdf");
    }

    // -------------------------------------------------------------------------
    // AddRangeAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddRangeAsync_SingleUpload_PersistsToDatabase()
    {
        // Arrange
        var uploads = new List<CourseUpload> { CreateUpload() };

        // Act
        await _sut.AddRangeAsync(uploads, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = Context.CourseUploads.ToList();
        saved.Count.ShouldBe(1);
    }

    [Fact]
    public async Task AddRangeAsync_MultipleUploads_PersistsAllToDatabase()
    {
        // Arrange
        var uploads = new List<CourseUpload>
        {
            CreateUpload("Upload 1"),
            CreateUpload("Upload 2"),
            CreateUpload("Upload 3")
        };

        // Act
        await _sut.AddRangeAsync(uploads, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = Context.CourseUploads.ToList();
        saved.Count.ShouldBe(3);
        saved.ShouldContain(u => u.Title == "Upload 1");
        saved.ShouldContain(u => u.Title == "Upload 2");
        saved.ShouldContain(u => u.Title == "Upload 3");
    }

    [Fact]
    public async Task AddRangeAsync_EmptyList_PersistsNothing()
    {
        // Arrange
        var uploads = new List<CourseUpload>();

        // Act
        await _sut.AddRangeAsync(uploads, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        Context.CourseUploads.ToList().ShouldBeEmpty();
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsUpload()
    {
        // Arrange
        var upload = await SeedUploadAsync();

        // Act
        var result = await _sut.GetByIdAsync(upload.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(upload.Id);
        result.Title.ShouldBe(upload.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithJobInclude_ReturnsUploadWithJobId()
    {
        // Arrange
        var upload = await SeedUploadAsync();
        var job = CourseJob.Create(upload.Id, "Pending");
        Context.CourseJobs.Add(job);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetByIdAsync(upload.Id, CancellationToken.None, u => u.Job!);

        // Assert
        result.ShouldNotBeNull();
        var linkedJob = Context.CourseJobs.FirstOrDefault(j => j.UploadId == upload.Id);
        linkedJob.ShouldNotBeNull();
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_MultipleUploads_ReturnsAll()
    {
        // Arrange
        await SeedUploadAsync("Upload A");
        await SeedUploadAsync("Upload B");

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(u => u.Title == "Upload A");
        result.ShouldContain(u => u.Title == "Upload B");
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsEmptyCollection()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.ShouldBeEmpty();
    }

    // -------------------------------------------------------------------------
    // Delete
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Delete_ExistingUpload_RemovesFromDatabase()
    {
        // Arrange
        var upload = await SeedUploadAsync();
        var tracked = await Context.CourseUploads.FindAsync(upload.Id);

        // Act
        _sut.Delete(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var deleted = await Context.CourseUploads.FindAsync(upload.Id);
        deleted.ShouldBeNull();
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Update_ExistingUpload_EntityRemainsIntact()
    {
        // Arrange
        var upload = await SeedUploadAsync();
        var tracked = await Context.CourseUploads.FindAsync(upload.Id);

        // Act
        _sut.Update(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.CourseUploads.FindAsync(upload.Id);
        refreshed.ShouldNotBeNull();
        refreshed!.Title.ShouldBe(upload.Title);
        refreshed.FileKey.ShouldBe(upload.FileKey);
        refreshed.UserId.ShouldBe(upload.UserId);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<CourseUpload> SeedUploadAsync(string title = "Test Title")
    {
        var upload = CreateUpload(title);
        Context.CourseUploads.Add(upload);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return upload;
    }

    private static CourseUpload CreateUpload(string title = "Test Title")
    {
        return CourseUpload.Create(Guid.NewGuid(), title, "file.pdf", "file-key", "application/pdf");
    }
}