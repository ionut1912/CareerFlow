using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Shouldly;

namespace CareerFlow.Core.Domain.Tests;

public class SystemDocumentsTests
{
    [Fact]
    public void Create_ValidData_CreateSystemDocument()
    {
        //Arrange
        var documenType = "test";
        var currentETag = "test";

        //Act
        var systemDocument = SystemDocument.Create(documenType, currentETag);

        //Arrange
        systemDocument.ShouldNotBeNull();
        systemDocument.DocumentType.ShouldBe(documenType);
        systemDocument.CurrentETag.ShouldBe(currentETag);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_InvalidType_ThrowsException(string type)
    {
        //Arrange
        var currentETag = "test";

        //Act
        var exception = Should.Throw<InvalidFieldException>(() => SystemDocument.Create(type, currentETag));

        //Assert
        exception.Message.ShouldBe("Tipul documentului este necesar");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_InvalidETag_ThrowsException(string etag)
    {
        //Arrange
        var type = "test";

        //Act
        var exception = Should.Throw<InvalidFieldException>(() => SystemDocument.Create(type, etag));

        //Assert
        exception.Message.ShouldBe("Etag-ul documentului este necesar");
    }

    [Fact]
    public void Update_DifferentEtagAndNotEmpty_UpdateSystemDocument()
    {
        //Arrange
        var currentETag = "test";
        var systemDocument = SystemDocument.Create("test", "etag");

        //Act
        systemDocument.Update(currentETag);

        //Assert
        systemDocument.CurrentETag.ShouldBe(currentETag);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Update_EmptyETag_ThrowsException(string etag)
    {
        //Arrange
        var systemDocument = SystemDocument.Create("test", "etag");

        //Act
        var exception = Should.Throw<InvalidFieldException>(() => systemDocument.Update(etag));

        //Assert
        exception.Message.ShouldBe("Etag-ul documentului este necesar");
    }

    [Fact]
    public void Update_SameEtag_ThrowsException()
    {
        //Arrange
        var currentETag = "test";
        var systemDocument = SystemDocument.Create("test", currentETag);

        //Act
        var exception = Should.Throw<DocumentEtagExistsException>(() => systemDocument.Update(currentETag));

        //Assert
        exception.Message.ShouldBe("Etag-ul documentului nu poate fi acelasi");
    }
}