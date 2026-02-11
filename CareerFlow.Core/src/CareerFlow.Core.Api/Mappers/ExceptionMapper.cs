using CareerFlow.Core.Domain.Exceptions;
using FluentValidation;
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
            ValidationException ex => CreateFromFluent(ex),
            AccountNotFoundException ex => Create(404, "Account Not Found", ex.Message),
            InvalidLegalDocTypeException ex => Create(400, "Invalid Legal Doc Type", ex.Message),
            PasswordNotMatchException ex => Create(400, "Password Not Match", ex.Message),
            UserAlreadyExistsException ex => Create(400, "User Already Exists", ex.Message),
            PasswordNotEmptyException ex => Create(400, "Password Not Empty", ex.Message),
            LegalDocNotFoundException ex => Create(404, "Legal Doc Not Found", ex.Message),
            InvalidRefreshTokenException ex => Create(401, "Invalid Refresh Token", ex.Message),
            TokenAlreadyUsedExcception ex => Create(400, "Token Already Used", ex.Message),
            TokenRevokedException ex => Create(400, "Token Revoked", ex.Message),
            TokenExpiredException ex => Create(401, "Token Expired", ex.Message),
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

    private static ProblemDetails CreateFromFluent(ValidationException ex)
    {
        var pd = Create(400, "Validation Error", "One or more validation errors occurred.");
        pd.Extensions["errors"] = ex.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );
        return pd;
    }
}