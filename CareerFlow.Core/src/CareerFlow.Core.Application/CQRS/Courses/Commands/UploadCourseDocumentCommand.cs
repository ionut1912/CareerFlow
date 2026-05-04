using CareerFlow.Core.Domain.Models.Course.Dto;

namespace CareerFlow.Core.Application.CQRS.Courses.Commands;

public sealed record UploadCourseDocumentCommand(Guid UserId, string Title, List<UploadFileDto> Files);
