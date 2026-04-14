using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Requests.Course;
using CareerFlow.Core.Domain.Models.Course.Response;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Endpoints;
using Shared.Api.Extensions;
using Shared.Api.Infrastructure;
using Wolverine;

namespace CareerFlow.Core.Api.Features.Course;

public class CourseEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(this)
            .RequireAuthorization();

        group.MapPost("/upload", UploadAsync).DisableAntiforgery();
        group.MapPost("/{courseId:guid}/chapters/{chapterId:guid}/finish", FinishChapterAsync);
        group.MapPost("/generate", GenerateCourseAsync);
    }

    private static async Task<Results<Accepted<UploadCoursesResponse>, UnauthorizedHttpResult>> UploadAsync(
        IMessageBus messageBus,
        [FromForm] UploadCourseDocumentRequest request,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var userId = httpContext.GetAccountId();
        if (userId == Guid.Empty) return TypedResults.Unauthorized();
        var command = request.ToUploadCourseDocumentCommand(userId);
        var response = await messageBus.InvokeAsync<UploadCoursesResponse>(command, ct);
        return TypedResults.Accepted((string?)null, value: response);
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> FinishChapterAsync(
        IMessageBus messageBus,
        [AsParameters] FinishChapterRequest request,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var userId = httpContext.GetAccountId();
        if (userId == Guid.Empty) return TypedResults.Unauthorized();
        var command = request.ToFinishChapterCommand(userId);
        await messageBus.InvokeAsync(command, ct);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<Guid>, UnauthorizedHttpResult>> GenerateCourseAsync(
        IMessageBus messageBus,
        CourseRequest courseRequest,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var userId = httpContext.GetAccountId();
        if (userId == Guid.Empty) return TypedResults.Unauthorized();
        var command = courseRequest.ToGenerateCourseCommand(userId);
        var result = await messageBus.InvokeAsync<Guid>(command, ct);
        return TypedResults.Ok(result);
    }
}