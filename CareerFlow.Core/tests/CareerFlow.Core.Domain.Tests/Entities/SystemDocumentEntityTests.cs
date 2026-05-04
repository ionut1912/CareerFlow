using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;

using Shouldly;

using Xunit;

namespace CareerFlow.Core.Domain.Tests.Entities;

public class SystemDocumentEntityTests
{
    [Fact]
    public void Create_ValidParameters_ReturnsDocument()
    {
        var doc = SystemDocument.Create("Terms", "etag123");

        doc.ShouldNotBeNull();
        doc.DocumentType.ShouldBe("Terms");
        doc.CurrentETag.ShouldBe("etag123");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidDocumentType_ThrowsInvalidFieldException(string? type) =>
        Should.Throw<InvalidFieldException>(() => SystemDocument.Create(type!, "etag"));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidEtag_ThrowsInvalidFieldException(string? etag) =>
        Should.Throw<InvalidFieldException>(() => SystemDocument.Create("Terms", etag!));

    [Fact]
    public void Update_NewEtag_UpdatesCurrentEtag()
    {
        var doc = SystemDocument.Create("Terms", "old");

        doc.Update("new");

        doc.CurrentETag.ShouldBe("new");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_InvalidEtag_ThrowsInvalidFieldException(string? etag)
    {
        var doc = SystemDocument.Create("Terms", "old");

        Should.Throw<InvalidFieldException>(() => doc.Update(etag!));
    }

    [Fact]
    public void Update_SameEtag_ThrowsDocumentEtagExistsException()
    {
        var doc = SystemDocument.Create("Terms", "same");

        Should.Throw<DocumentEtagExistsException>(() => doc.Update("same"));
    }
}
