using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Application.Validators.Course;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Application.Tests.Validators.Course;

public class UploadCourseDocumentCommandValidatorTests
{
    private readonly UploadCourseDocumentCommandValidator _sut = new();

    private static IFormFileCollection CreateMockFiles(int count = 1)
    {
        var collection = new FormFileCollection();
        
        for (int i = 0; i < count; i++)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns($"test{i}.pdf");
            collection.Add(fileMock.Object);
        }

        return collection;
    }

    [Fact]
    public void Validate_ValidCommand_PassesWithNoErrors()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "My Course", CreateMockFiles());

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_EmptyTitle_HasErrorWithRomanianMessage(string? title)
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), title!, CreateMockFiles());

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Titlul este necesar");
    }

    [Fact]
    public void Validate_NullFiles_HasErrorWithRomanianMessage()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "Title", null!);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Files)
              .WithErrorMessage("Files sunt necesare");
    }

    [Fact]
    public void Validate_BothTitleAndFilesInvalid_HasTwoErrors()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "", null!);

        var result = _sut.TestValidate(command);

        result.Errors.Count.ShouldBe(2);
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Files);
    }

    [Fact]
    public void Validate_MultipleFiles_PassesValidation()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "Title", CreateMockFiles(3));

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}