using CareerFlow.Core.Domain.Models.Course.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareerFlow.Core.Application.CQRS.Courses.Commands;

public sealed record UploadCourseDocumentCommand(Guid UserId,[FromForm] string Title, List<UploadFileDto> Files);
