using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public sealed class CourseJob : Entity
{
    private CourseJob()
    {
        Status = JobStatus.Pending;
    }

    private CourseJob(Guid uploadId, JobStatus status)
    {
        if (uploadId == Guid.Empty) throw new InvalidFieldException("Upload id pt job este necesar");

        UploadId = uploadId;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UploadId { get; private set; }
    public Guid? CourseId { get; private set; }
    public JobStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public CourseUpload? Upload { get; private set; }
    public Course? Course { get; private set; }

    public static CourseJob Create(Guid uploadId, string status)
    {
        return new CourseJob(uploadId, JobStatus.FromString(status));
    }

    public void SetCourseId(Guid courseId)
    {
        CourseId = courseId;
    }

    public void SetErrorMessage(string? errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public void Update(JobStatus status)
    {
        Status = status;

        if (status == JobStatus.Processing) StartedAt = DateTime.UtcNow;

        if (status == JobStatus.Done || status == JobStatus.Failed) CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}