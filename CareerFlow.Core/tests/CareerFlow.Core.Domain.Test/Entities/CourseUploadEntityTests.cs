using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Domain.Test;
public class CourseUploadEntityTests
{
    [Fact]
    public void Create_ValidParameters_ReturnsCourseUpload()
    {
        var userId = Guid.NewGuid();
 
        var upload = CourseUpload.Create(userId, "Title", "file.pdf", "key/file.pdf", "pdf");
 
        upload.ShouldNotBeNull();
        upload.UserId.ShouldBe(userId);
        upload.Title.ShouldBe("Title");
        upload.FileName.ShouldBe("file.pdf");
        upload.FileKey.ShouldBe("key/file.pdf");
        upload.FileType.ShouldBe("pdf");
    }
 
    [Fact]
    public void Create_SetsCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
 
        var upload = CourseUpload.Create(Guid.NewGuid(), "Title", "file.pdf", "key", "pdf");
 
        upload.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
    }
 
    [Fact]
    public void Create_EmptyUserId_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            CourseUpload.Create(Guid.Empty, "Title", "file.pdf", "key", "pdf"));
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidTitle_ThrowsInvalidFieldException(string? title)
    {
        Should.Throw<InvalidFieldException>(() =>
            CourseUpload.Create(Guid.NewGuid(), title!, "file.pdf", "key", "pdf"));
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidFileName_ThrowsInvalidFieldException(string? fileName)
    {
        Should.Throw<InvalidFieldException>(() =>
            CourseUpload.Create(Guid.NewGuid(), "Title", fileName!, "key", "pdf"));
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidFileKey_ThrowsInvalidFieldException(string? fileKey)
    {
        Should.Throw<InvalidFieldException>(() =>
            CourseUpload.Create(Guid.NewGuid(), "Title", "file.pdf", fileKey!, "pdf"));
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidFileType_ThrowsInvalidFieldException(string? fileType)
    {
        Should.Throw<InvalidFieldException>(() =>
            CourseUpload.Create(Guid.NewGuid(), "Title", "file.pdf", "key", fileType!));
    }
}