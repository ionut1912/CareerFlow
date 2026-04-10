using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Domain.Test;

public class CourseJobEntityTests
{
    [Fact]
    public void Create_ValidParameters_ReturnsCourseJob()
    {
        var uploadId = Guid.NewGuid();

        var job = CourseJob.Create(uploadId, "pending");

        job.ShouldNotBeNull();
        job.UploadId.ShouldBe(uploadId);
    }

    [Fact]
    public void Create_SetsCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var job = CourseJob.Create(Guid.NewGuid(), "pending");

        job.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public void Create_EmptyUploadId_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() => CourseJob.Create(Guid.Empty, "pending"));
    }

    [Fact]
    public void Create_InvalidStatus_ThrowsInvalidJobStatusException()
    {
        Should.Throw<InvalidJobStatusException>(() =>
            CourseJob.Create(Guid.NewGuid(), "totally_invalid_status"));
    }

    [Fact]
    public void Update_ProcessingStatus_SetsStartedAt()
    {
        var job = CourseJob.Create(Guid.NewGuid(), "pending");
        var before = DateTime.UtcNow.AddSeconds(-1);

        job.Update(JobStatus.Processing);

        job.StartedAt.ShouldNotBeNull();
        ((DateTime)job.StartedAt!).ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public void Update_DoneStatus_SetsCompletedAt()
    {
        var job = CourseJob.Create(Guid.NewGuid(), "pending");
        var before = DateTime.UtcNow.AddSeconds(-1);

        job.Update(JobStatus.Done);

        job.CompletedAt.ShouldNotBeNull();
        ((DateTime)job.CompletedAt!).ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public void Update_FailedStatus_SetsCompletedAt()
    {
        var job = CourseJob.Create(Guid.NewGuid(), "pending");
        var before = DateTime.UtcNow.AddSeconds(-1);

        job.Update(JobStatus.Failed);

        job.CompletedAt.ShouldNotBeNull();
        ((DateTime)job.CompletedAt!).ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public void Update_PendingStatus_DoesNotSetStartedAt()
    {
        var job = CourseJob.Create(Guid.NewGuid(), "pending");

        job.Update(JobStatus.Pending);

        job.StartedAt.ShouldBeNull();
    }

    [Fact]
    public void Update_SetsUpdatedAt()
    {
        var job = CourseJob.Create(Guid.NewGuid(), "pending");
        var before = DateTime.UtcNow.AddSeconds(-1);

        job.Update(JobStatus.Processing);

        ((DateTime)job.UpdatedAt!).ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public void SetCourseId_ValidId_SetsCourseId()
    {
        var job = CourseJob.Create(Guid.NewGuid(), "pending");
        var courseId = Guid.NewGuid();

        job.SetCourseId(courseId);

        job.CourseId.ShouldBe(courseId);
    }

    [Fact]
    public void SetErrorMessage_ValidMessage_SetsErrorMessage()
    {
        var job = CourseJob.Create(Guid.NewGuid(), "pending");

        job.SetErrorMessage("Something failed");

        job.ErrorMessage.ShouldBe("Something failed");
    }

    [Fact]
    public void SetErrorMessage_Null_SetsNull()
    {
        var job = CourseJob.Create(Guid.NewGuid(), "pending");

        job.SetErrorMessage(null);

        job.ErrorMessage.ShouldBeNull();
    }
}