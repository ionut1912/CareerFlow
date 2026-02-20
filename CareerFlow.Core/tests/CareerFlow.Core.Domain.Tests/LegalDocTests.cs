using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Shouldly;

namespace CareerFlow.Core.Domain.Tests;

public class LegalDocTests
{
    [Theory]
    [InlineData("PrivacyPolicy")]
    [InlineData("privacyPoLICY")]
    [InlineData("termsandconditions")]
    [InlineData("TermsAndCoNditions")]
    public void LegalDoc_ValidFields_CreateLegalEntity(string type)
    {
        //Arrange
        var content = "testContent";

        //Act
        var legalDoc = LegalDoc.Create(content, type);

        //Assert
        legalDoc.Content.ShouldBe(content);
        legalDoc.Type.ShouldBe(LegalDocType.FromString(type));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void LegalDoc_InvalidContent_ThrowsException(string content)
    {
        //Arrange
        var type = "PrivacyPolicy";

        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            LegalDoc.Create(content, type));

        //Assert
        exception.Message.ShouldBe("Continutul este invalid");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void LegalDoc_EmptyType_ThrowsException(string type)
    {
        //Arrange
        var content = "test";

        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            LegalDoc.Create(content, type));

        //Assert
        exception.Message.ShouldBe("Tipul este invalid");
    }

    [Theory]
    [InlineData("termsAndCond")]
    [InlineData("Privacypol")]
    public void LegalDoc_InvalidType_ThrowsException(string type)
    {
        //Arrange
        var content = "test";

        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            LegalDoc.Create(content, type));

        //Assert
        exception.Message.ShouldBe($"Tipul este invalid:{type}");
    }

    [Fact]
    public void LegalDocUpdate_ValidFields_UpdateDoc()
    {
        //Arrange
        var content = "testContent";
        var type = "PrivacyPolicy";
        var newContent = "newContent";
        var newType = "TermsAndConditions";
        var legalDoc = LegalDoc.Create(content, type);

        //Act
        legalDoc.Update(newContent, newType);

        //Assert
        legalDoc.Content.ShouldBe(newContent);
        legalDoc.Type.ShouldBe(LegalDocType.FromString(newType));
    }

    [Theory]
    [InlineData("termsAndCond")]
    [InlineData("Privacypol")]
    public void LegalDocUpdate_InvalidType_ThrowsException(string newType)
    {
        //Arrange
        var content = "test";
        var newContent = "newContent";
        var type = "PrivacyPolicy";
        var legalDoc = LegalDoc.Create(content, type);
        //Act
        var exception = Should.Throw<InvalidFieldException>(() =>
            legalDoc.Update(newContent, newType));

        //Assert
        exception.Message.ShouldBe($"Tipul este invalid:{newType}");
    }
}