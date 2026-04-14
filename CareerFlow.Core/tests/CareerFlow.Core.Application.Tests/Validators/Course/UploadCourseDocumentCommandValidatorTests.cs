using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Application.Validators.Course;
using CareerFlow.Core.Domain.Constants;
using CareerFlow.Core.Domain.Models.Course.Dto;
using FluentValidation.TestHelper;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Validators.Course;

public class UploadCourseDocumentCommandValidatorTests
{
    private readonly UploadCourseDocumentCommandValidator _sut = new();

    private static UploadFileDto CreateMockFile(
        string fileName = "test.pdf",
        string contentType = "application/pdf",
        byte[]? content = null)
    {
        return new UploadFileDto(
            fileName,
            contentType,
            new MemoryStream(content ?? [1, 2, 3])
        );
    }

    private static List<UploadFileDto> CreateMockFiles(int count = 1) =>
        Enumerable.Range(0, count)
            .Select(i => CreateMockFile($"test{i}.pdf"))
            .ToList();

    // -------------------------------------------------------------------------
    // Happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_ValidCommand_PassesWithNoErrors()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "My Course", CreateMockFiles());

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_MultipleFiles_PassesValidation()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "Title", CreateMockFiles(3));

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_MaxAllowedFileCount_PassesValidation()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "Title", CreateMockFiles(CourseConstants.MaxFiles));

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_OversizedFile_PassesValidation()
    {
        // Size is a service-layer concern — validator does not reject oversized files
        var oversized = new byte[CourseConstants.MaxFileSizeBytes + 1];
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "Title",
            [CreateMockFile(content: oversized)]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("application/octet-stream")]
    public void Validate_DisallowedContentType_PassesValidation(string contentType)
    {
        // Content type is a service-layer concern — validator does not reject by content type
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "Title",
            [CreateMockFile(contentType: contentType)]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // -------------------------------------------------------------------------
    // Title
    // -------------------------------------------------------------------------

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

    // -------------------------------------------------------------------------
    // Files collection
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_NullFiles_HasErrorWithRomanianMessage()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "Title", null!);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Files)
            .WithErrorMessage("Files sunt necesare");
    }

    [Fact]
    public void Validate_EmptyFileList_HasError()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "Title", []);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Files)
            .WithErrorMessage("Files sunt necesare");
    }

    [Fact]
    public void Validate_TooManyFiles_HasError()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "Title",
            CreateMockFiles(CourseConstants.MaxFiles + 1));

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Files)
            .WithErrorMessage($"Numărul maxim de fișiere este {CourseConstants.MaxFiles}");
    }

    // -------------------------------------------------------------------------
    // Per-file: content
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_EmptyFileContent_HasError()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "Title",
            [CreateMockFile(content: [])]);

        var result = _sut.TestValidate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Fișierul nu poate fi gol");
    }

    // -------------------------------------------------------------------------
    // Combined
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_BothTitleAndFilesInvalid_HasTwoErrors()
    {
        var command = new UploadCourseDocumentCommand(Guid.NewGuid(), "", null!);

        var result = _sut.TestValidate(command);

        result.Errors.Count.ShouldBe(2);
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Files);
    }
}