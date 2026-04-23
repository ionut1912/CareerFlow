using CareerFlow.Core.Domain.Exceptions;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using Shared.Api.Abstractions;
using Shared.Domain.Exceptions;

namespace CareerFlow.Core.Api.Mappers;

public sealed partial class ExceptionMapper(ILogger<ExceptionMapper> logger) : IExceptionProblemDetailsMapper
{
    private readonly ILogger<ExceptionMapper> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public bool TryMap(Exception exception, out ProblemDetails problemDetails)
    {
        ArgumentNullException.ThrowIfNull(exception);

        problemDetails = exception switch
        {
            ValidationException ex => CreateFromFluent(ex),
            InvalidLearningTypeException ex => Create(400, "Invalid Learning Type", ex.Message),
            InvalidJobStatusException ex => Create(400, "Invalid Job Status", ex.Message),
            DomainAlreadyExistsException ex => Create(400, "Domain Already Exists", ex.Message),
            UserProfileNotFoundException ex => Create(404, "User Profile Not Found", ex.Message),
            InvalidUserTypeException ex => Create(400, "Invalid User Type", ex.Message),
            LearningTypeAlreadyExistsException ex => Create(400, "Learning Type Already Exists", ex.Message),
            DocumentEtagExistsException ex => Create(400, "Document Etag Exists", ex.Message),
            AccountNotFoundException ex => Create(404, "Account Not Found", ex.Message),
            InvalidFieldException ex => Create(400, "Invalid Field", ex.Message),
            PasswordNotMatchException ex => Create(400, "Password Not Match", ex.Message),
            UserAlreadyExistsException ex => Create(400, "User Already Exists", ex.Message),
            InvalidRefreshTokenException ex => Create(401, "Invalid Refresh Token", ex.Message),
            TokenAlreadyUsedException ex => Create(400, "Token Already Used", ex.Message),
            TokenRevokedException ex => Create(400, "Token Revoked", ex.Message),
            LegalDocInvalidTypeException ex => Create(400, "Legal Doc Invalid Type", ex.Message),
            LegalDocNotFoundException ex => Create(404, "Legal Doc Not Found", ex.Message),
            ChapterNotFoundException ex => Create(404, "Chapter Not Found", ex.Message),
            CustomValidationException ex => CreateValidation(ex),
            _ => Create(500, "Internal Server Error", "An unexpected error occurred")
        };

        LogMappedException(_logger, exception, exception.GetType().Name, problemDetails.Status, problemDetails.Title);

        return true;
    }

    private static ProblemDetails Create(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };

    private static ProblemDetails CreateValidation(CustomValidationException ex)
    {
        ProblemDetails pd = Create(400, "Validation Error", "One or more validation errors occurred.");
        pd.Extensions["errors"] = ex.ValidationErrors;
        return pd;
    }

    private static ProblemDetails CreateFromFluent(ValidationException ex)
    {
        ProblemDetails pd = Create(400, "Validation Error", "One or more validation errors occurred.");
        pd.Extensions["errors"] = ex.Errors
            .GroupBy(x => string.IsNullOrEmpty(x.PropertyName) ? "_general" : x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );
        return pd;
    }

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Mapped exception {ExceptionType} to ProblemDetails {Status} - {Title}")]
    private static partial void LogMappedException(
        ILogger logger,
        Exception exception,
        string exceptionType,
        int? status,
        string? title);
}
