using CareerFlow.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Abstractions;
using Shared.Domain.Exceptions;
namespace CareerFlow.Core.Api.Mappers;

public sealed class ExceptionMapper : IExceptionProblemDetailsMapper
{
    private readonly ILogger<ExceptionMapper> _logger;

    public ExceptionMapper(ILogger<ExceptionMapper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool TryMap(Exception exception, out ProblemDetails problemDetails)
    {
        ArgumentNullException.ThrowIfNull(exception);

        problemDetails = exception switch
        {
            AccountNotFoundException ex => Create(404, "Account Not Found", ex.Message),
            PasswordNotMatchException ex => Create(400, "Password Not Match", ex.Message),
            UserAlreadyExistsException ex => Create(400, "User Already Exists", ex.Message),
            PasswordNotEmptyException ex => Create(400, "Password Not Empty", ex.Message),
            TermsAndConditionsNotFoundException ex => Create(404, "Terms And Conditions Not Found", ex.Message),
            PrivacyPolicyNotFoundException ex => Create(404, "Privacy Policy Not Found", ex.Message),
            CustomValidationException ex => CreateValidation(ex),

            _ => Create(500, "Internal Server Error", "An unexpected error occurred")
        };

        _logger.LogError(
            exception,
            "Mapped exception {ExceptionType} to ProblemDetails {Status} - {Title}",
            exception.GetType().Name,
            problemDetails.Status,
            problemDetails.Title);

        return problemDetails != null;
    }

    private static ProblemDetails Create(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };

    private static ProblemDetails CreateValidation(CustomValidationException ex)
    {
        var pd = Create(400, "Validation Error", "One or more validation errors occurred.");
        pd.Extensions["errors"] = ex.ValidationErrors;
        return pd;
    }
}
