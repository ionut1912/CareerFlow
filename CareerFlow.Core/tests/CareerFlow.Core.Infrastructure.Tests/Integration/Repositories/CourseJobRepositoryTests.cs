using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using CareerFlow.Core.Infrastructure.Persistence.Repositories;
using CareerFlow.Core.Infrastructure.Tests.Integration.Setup;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class CourseJobRepositoryTests : BaseRepositoryTest, IAsyncLifetime
{
    private readonly CourseJobRepository _sut;

    public CourseJobRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new CourseJobRepository(Context.CourseJobs);
    }

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Context.CourseJobs.RemoveRange(Context.CourseJobs);
        Context.CourseUploads.RemoveRange(Context.CourseUploads);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task AddRangeAsync_SingleJob_PersistsToDatabase()
    {
        // Arrange
        var uploadId = await SeedUploadAsync();
        var jobs = new List<CourseJob> { CourseJob.Create(uploadId, "Pending") };

        // Act
        await _sut.AddRangeAsync(jobs, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.CourseJobs.ToListAsync();
        saved.Count.ShouldBe(1);
        saved[0].UploadId.ShouldBe(uploadId);
        saved[0].Status.ShouldBe(JobStatus.Pending);
    }

    [Fact]
    public async Task AddRangeAsync_MultipleJobs_PersistsAllToDatabase()
    {
        // Arrange
        var uploadId1 = await SeedUploadAsync();
        var uploadId2 = await SeedUploadAsync();
        var jobs = new List<CourseJob>
        {
            CourseJob.Create(uploadId1, "Pending"),
            CourseJob.Create(uploadId2, "Pending")
        };

        // Act
        await _sut.AddRangeAsync(jobs, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.CourseJobs.ToListAsync();
        saved.Count.ShouldBe(2);
        saved.ShouldContain(j => j.UploadId == uploadId1);
        saved.ShouldContain(j => j.UploadId == uploadId2);
    }

    [Fact]
    public async Task AddRangeAsync_EmptyList_PersistsNothing()
    {
        // Arrange
        var jobs = new List<CourseJob>();

        // Act
        await _sut.AddRangeAsync(jobs, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.CourseJobs.ToListAsync();
        saved.ShouldBeEmpty();
    }

    [Fact]
    public async Task AddRangeAsync_NewJob_HasPendingStatusByDefault()
    {
        // Arrange
        var uploadId = await SeedUploadAsync();
        var jobs = new List<CourseJob> { CourseJob.Create(uploadId, "Pending") };

        // Act
        await _sut.AddRangeAsync(jobs, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.CourseJobs.FirstAsync();
        saved.Status.ShouldBe(JobStatus.Pending);
        saved.StartedAt.ShouldBeNull();
        saved.CompletedAt.ShouldBeNull();
        saved.ErrorMessage.ShouldBeNull();
        saved.CourseId.ShouldBeNull();
    }

    [Fact]
    public async Task AddAsync_Job_PersistsToDatabase()
    {
        // Arrange
        var uploadId = await SeedUploadAsync();
        var job = CourseJob.Create(uploadId, "Pending");

        // Act
        await _sut.AddAsync(job, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.CourseJobs.FindAsync(job.Id);
        saved.ShouldNotBeNull();
        saved!.UploadId.ShouldBe(uploadId);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsJob()
    {
        // Arrange
        var job = await SeedJobAsync();

        // Act
        var result = await _sut.GetByIdAsync(job.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(job.Id);
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
    public async Task GetByIdAsync_WithUploadInclude_ReturnsJobWithUpload()
    {
        // Arrange
        var job = await SeedJobAsync();

        // Act
        var result = await _sut.GetByIdAsync(job.Id, CancellationToken.None, j => j.Upload!);

        // Assert
        result.ShouldNotBeNull();
        result!.Upload.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultipleJobs_ReturnsAll()
    {
        // Arrange
        var uploadId1 = await SeedUploadAsync();
        var uploadId2 = await SeedUploadAsync();
        await _sut.AddRangeAsync(new List<CourseJob>
        {
            CourseJob.Create(uploadId1, "Pending"),
            CourseJob.Create(uploadId2, "Pending")
        }, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllAsync_WithUploadInclude_ReturnsJobsWithUpload()
    {
        // Arrange
        await SeedJobAsync();

        // Act
        var result = (await _sut.GetAllAsync(CancellationToken.None, j => j.Upload!)).ToList();

        // Assert
        result.ShouldAllBe(j => j.Upload != null);
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
    public async Task Delete_ExistingJob_RemovesFromDatabase()
    {
        // Arrange
        var job = await SeedJobAsync();
        var tracked = await Context.CourseJobs.FindAsync(job.Id);

        // Act
        _sut.Delete(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var deleted = await Context.CourseJobs.FindAsync(job.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Update_StatusChange_IsPersisted()
    {
        // Arrange
        var job = await SeedJobAsync();
        var tracked = await Context.CourseJobs.FindAsync(job.Id);
        tracked!.Update(JobStatus.Processing);

        // Act
        _sut.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.CourseJobs.FindAsync(job.Id);
        refreshed!.Status.ShouldBe(JobStatus.Processing);
        refreshed.StartedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Update_DoneStatus_SetsCompletedAt()
    {
        // Arrange
        var job = await SeedJobAsync();
        var tracked = await Context.CourseJobs.FindAsync(job.Id);
        tracked!.Update(JobStatus.Done);

        // Act
        _sut.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.CourseJobs.FindAsync(job.Id);
        refreshed!.Status.ShouldBe(JobStatus.Done);
        refreshed.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Update_FailedStatus_SetsCompletedAt()
    {
        // Arrange
        var job = await SeedJobAsync();
        var tracked = await Context.CourseJobs.FindAsync(job.Id);
        tracked!.Update(JobStatus.Failed);

        // Act
        _sut.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.CourseJobs.FindAsync(job.Id);
        refreshed!.Status.ShouldBe(JobStatus.Failed);
        refreshed.CompletedAt.ShouldNotBeNull();
    }

    private async Task<CourseJob> SeedJobAsync()
    {
        var uploadId = await SeedUploadAsync();
        var job = CourseJob.Create(uploadId, "Pending");
        Context.CourseJobs.Add(job);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return job;
    }

    private async Task<Guid> SeedUploadAsync()
    {
        var upload = CourseUpload.Create(Guid.NewGuid(), "Test Title", "file.pdf", "file-key", "application/pdf");
        Context.CourseUploads.Add(upload);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return upload.Id;
    }
}