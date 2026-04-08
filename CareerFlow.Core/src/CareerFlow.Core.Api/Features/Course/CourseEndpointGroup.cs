using CareerFlow.Core.Application.Requests;
using CareerFlow.Core.Domain.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Endpoints;
using Shared.Api.Extensions;
using Shared.Api.Infrastructure;

namespace CareerFlow.Core.Api.Features.Course;

public class CourseEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(this)
            .RequireAuthorization();

        group.MapPost("/upload", UploadAsync).DisableAntiforgery();
        group.MapGet("/jobs", GetJobStatusesAsync);
        group.MapPost("/{courseId:guid}/chapters/{chapterId:guid}/finish", FinishChapterAsync);
    }

    private static async Task<IResult> UploadAsync(
        [FromForm] string title,
        [FromForm] IFormFileCollection files, // 👈 add [FromForm]
        ICourseService service,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var userId = httpContext.GetAccountId();
        if (userId == Guid.Empty) return Results.Unauthorized();
        var request = new UploadCoursesRequest { Files = files, Title = title };
        var response = await service.UploadManyAsync(userId, request, ct);
        return Results.Accepted(value: response);
    }

    private static async Task<IResult> GetJobStatusesAsync(
        [FromQuery] Guid[] jobIds,
        ICourseService service,
        CancellationToken ct)
    {
        var result = await service.GetJobStatusesAsync(jobIds, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> FinishChapterAsync(
        Guid courseId,
        Guid chapterId,
        ICourseService service,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var userId = httpContext.GetAccountId();
        if (userId == Guid.Empty) return Results.Unauthorized();
        await service.FinishChapterAsync(userId, courseId, chapterId, ct);
        return Results.NoContent();
    }
}