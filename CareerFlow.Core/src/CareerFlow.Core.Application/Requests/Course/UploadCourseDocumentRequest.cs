using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareerFlow.Core.Application.Requests.Course;

public sealed record UploadCourseDocumentRequest([FromForm] string Title, [FromForm] IFormFileCollection Files);