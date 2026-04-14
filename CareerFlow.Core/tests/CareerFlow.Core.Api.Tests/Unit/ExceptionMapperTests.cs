using CareerFlow.Core.Api.Mappers;
using CareerFlow.Core.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Domain.Common;
using Shared.Domain.Exceptions;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.Tests.Unit;

public class ExceptionMapperTests
{
    private readonly Mock<ILogger<ExceptionMapper>> _loggerMock = new();
    private readonly ExceptionMapper _sut;

    public ExceptionMapperTests()
    {
        _sut = new ExceptionMapper(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new ExceptionMapper(null!));
    }

    [Fact]
    public void TryMap_NullException_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => _sut.TryMap(null!, out _));
    }

    [Fact]
    public void TryMap_ValidationException_Returns400()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is required"),
            new("Name", "Name is required")
        };
        var ex = new ValidationException(failures);

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Validation Error");
        pd.Extensions.ContainsKey("errors").ShouldBeTrue();
    }

    [Fact]
    public void TryMap_ValidationException_GroupsErrorsByPropertyName()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is required"),
            new("Email", "Email is invalid"),
            new("", "General error")
        };
        var ex = new ValidationException(failures);

        _sut.TryMap(ex, out var pd);

        var errors = (Dictionary<string, string[]>)pd.Extensions["errors"]!;
        errors["Email"].Length.ShouldBe(2);
        errors.ContainsKey("_general").ShouldBeTrue();
    }

    [Fact]
    public void TryMap_ValidationException_EmptyPropertyName_UsesGeneralKey()
    {
        var failures = new List<ValidationFailure> { new("", "Some general error") };
        var ex = new ValidationException(failures);

        _sut.TryMap(ex, out var pd);

        var errors = (Dictionary<string, string[]>)pd.Extensions["errors"]!;
        errors.ContainsKey("_general").ShouldBeTrue();
        errors["_general"].ShouldContain("Some general error");
    }

    [Fact]
    public void TryMap_CustomValidationException_Returns400WithValidationErrors()
    {
        var validationErrors = new List<ValidationError>
        {
            new("Field1", "Field1 is required"),
            new("Field2", "Field2 is invalid")
        };
        var ex = new CustomValidationException(validationErrors);

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Validation Error");
        pd.Extensions.ContainsKey("errors").ShouldBeTrue();
        var mapped = (List<ValidationError>)pd.Extensions["errors"]!;
        mapped.Count.ShouldBe(2);
        mapped[0].Property.ShouldBe("Field1");
        mapped[1].Property.ShouldBe("Field2");
    }

    [Fact]
    public void TryMap_CustomValidationException_NullErrors_DoesNotThrow()
    {
        var ex = new CustomValidationException(null!);

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
    }

    [Fact]
    public void TryMap_InvalidLearningTypeException_Returns400()
    {
        var ex = new InvalidLearningTypeException("bad type");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Invalid Learning Type");
        pd.Detail.ShouldBe("bad type");
    }

    [Fact]
    public void TryMap_InvalidJobStatusException_Returns400()
    {
        var ex = new InvalidJobStatusException("bad status");
 
        var result = _sut.TryMap(ex, out var pd);
 
        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Invalid Job Status");
    }
 
   

    [Fact]
    public void TryMap_DomainAlreadyExistsException_Returns400()
    {
        var ex = new DomainAlreadyExistsException("exists");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Domain Already Exists");
    }

    [Fact]
    public void TryMap_UserProfileNotFoundException_Returns404()
    {
        var ex = new UserProfileNotFoundException("not found");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(404);
        pd.Title.ShouldBe("User Profile Not Found");
    }

    [Fact]
    public void TryMap_InvalidUserTypeException_Returns400()
    {
        var ex = new InvalidUserTypeException("invalid");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Invalid User Type");
    }

    [Fact]
    public void TryMap_LearningTypeAlreadyExistsException_Returns400()
    {
        var ex = new LearningTypeAlreadyExistsException("exists");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Learning Type Already Exists");
    }

    [Fact]
    public void TryMap_DocumentEtagExistsException_Returns400()
    {
        var ex = new DocumentEtagExistsException("etag exists");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Document Etag Exists");
    }

    [Fact]
    public void TryMap_AccountNotFoundException_Returns404()
    {
        var ex = new AccountNotFoundException("not found");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(404);
        pd.Title.ShouldBe("Account Not Found");
    }

    [Fact]
    public void TryMap_InvalidFieldException_Returns400()
    {
        var ex = new InvalidFieldException("invalid field");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Invalid Field");
        pd.Detail.ShouldBe("invalid field");
    }

    [Fact]
    public void TryMap_PasswordNotMatchException_Returns400()
    {
        var ex = new PasswordNotMatchException("no match");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Password Not Match");
    }

    [Fact]
    public void TryMap_UserAlreadyExistsException_Returns400()
    {
        var ex = new UserAlreadyExistsException("exists");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("User Already Exists");
    }

    [Fact]
    public void TryMap_InvalidRefreshTokenException_Returns401()
    {
        var ex = new InvalidRefreshTokenException("invalid");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(401);
        pd.Title.ShouldBe("Invalid Refresh Token");
    }

    [Fact]
    public void TryMap_TokenAlreadyUsedExcception_Returns400()
    {
        var ex = new TokenAlreadyUsedExcception("used");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Token Already Used");
    }

    [Fact]
    public void TryMap_TokenRevokedException_Returns400()
    {
        var ex = new TokenRevokedException("revoked");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Token Revoked");
    }

    [Fact]
    public void TryMap_LegalDocInvalidTypeException_Returns400()
    {
        var ex = new LegalDocInvalidTypeException("invalid");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(400);
        pd.Title.ShouldBe("Legal Doc Invalid Type");
    }

    [Fact]
    public void TryMap_LegalDocNotFoundException_Returns404()
    {
        var ex = new LegalDocNotFoundException("not found");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(404);
        pd.Title.ShouldBe("Legal Doc Not Found");
    }
 
    [Fact]

    public void TryMap_UnknownException_Returns500()
    {
        var ex = new Exception("unexpected");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.Status.ShouldBe(500);
        pd.Title.ShouldBe("Internal Server Error");
        pd.Detail.ShouldBe("An unexpected error occurred");
    }

    [Fact]
    public void TryMap_AnyException_LogsError()
    {
        var ex = new Exception("test");

        _sut.TryMap(ex, out _);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void TryMap_AnyException_ReturnsTrueWithNonNullProblemDetails()
    {
        var ex = new InvalidOperationException("op error");

        var result = _sut.TryMap(ex, out var pd);

        result.ShouldBeTrue();
        pd.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(typeof(InvalidLearningTypeException), 400)]
    [InlineData(typeof(InvalidJobStatusException), 400)]
    [InlineData(typeof(AccountNotFoundException), 404)]
    [InlineData(typeof(UserProfileNotFoundException), 404)]
    [InlineData(typeof(ChapterNotFoundException), 404)]
    [InlineData(typeof(LegalDocNotFoundException), 404)]
    [InlineData(typeof(InvalidRefreshTokenException), 401)]
    public void TryMap_KnownException_ReturnsExpectedStatusCode(Type exType, int expectedStatus)
    {
        var ex = (Exception)Activator.CreateInstance(exType, "msg")!;

        _sut.TryMap(ex, out var pd);

        pd.Status.ShouldBe(expectedStatus);
    }
}