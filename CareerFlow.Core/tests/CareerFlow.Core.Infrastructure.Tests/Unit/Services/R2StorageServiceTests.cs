using Amazon.S3;
using Amazon.S3.Model;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Services;

public class R2StorageServiceTests
{
    private const string BucketName = "test-bucket";
    private const string AccountId="test-account-id";
    private const string AccessKey="test-access-key";
    private const string SecretKey = "test-secret-key";

    private readonly Mock<IAmazonS3> _s3 = new();
    private readonly R2StorageService _sut;

    public R2StorageServiceTests()
    {
        IOptions<R2Settings> options = Options.Create(new R2Settings {AccountId = AccountId,AccessKey = AccessKey,SecretKey = SecretKey,BucketName = BucketName });
        _sut = new R2StorageService(_s3.Object, options);
    }

    // -------------------------------------------------------------------------
    // UploadAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UploadAsync_ValidInput_CallsPutObjectWithCorrectBucketAndContentType()
    {
        PutObjectRequest? captured = null;
        _s3.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutObjectRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(new PutObjectResponse());

        await _sut.UploadAsync(new MemoryStream([1, 2, 3]), "doc.pdf", "application/pdf");

        captured.ShouldNotBeNull();
        captured.BucketName.ShouldBe(BucketName);
        captured.ContentType.ShouldBe("application/pdf");
        captured.AutoCloseStream.ShouldBeFalse();
        captured.DisablePayloadSigning?.ShouldBeTrue();
        captured.UseChunkEncoding.ShouldBeFalse();
    }

    [Fact]
    public async Task UploadAsync_ValidInput_ReturnsKeyWithCoursesPrefixAndFileName()
    {
        _s3.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        string key = await _sut.UploadAsync(new MemoryStream([1, 2, 3]), "doc.pdf", "application/pdf");

        key.ShouldStartWith("courses/");
        key.ShouldEndWith("/doc.pdf");
    }

    [Fact]
    public async Task UploadAsync_CalledTwice_ReturnsDifferentKeys()
    {
        _s3.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        string key1 = await _sut.UploadAsync(new MemoryStream([1]), "doc.pdf", "application/pdf");
        string key2 = await _sut.UploadAsync(new MemoryStream([1]), "doc.pdf", "application/pdf");

        key1.ShouldNotBe(key2);
    }

    [Fact]
    public async Task UploadAsync_PassesStreamToRequest()
    {
        var stream = new MemoryStream([1, 2, 3]);
        PutObjectRequest? captured = null;
        _s3.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutObjectRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(new PutObjectResponse());

        await _sut.UploadAsync(stream, "doc.pdf", "application/pdf");

        captured!.InputStream.ShouldBeSameAs(stream);
    }

    [Fact]
    public async Task UploadAsync_S3Throws_PropagatesException()
    {
        _s3.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("S3 error"));

        await Should.ThrowAsync<AmazonS3Exception>(() =>
            _sut.UploadAsync(new MemoryStream([1]), "doc.pdf", "application/pdf"));
    }

    // -------------------------------------------------------------------------
    // DownloadAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DownloadAsync_ValidKey_CallsGetObjectWithCorrectBucketAndKey()
    {
        const string fileKey = "courses/abc/doc.pdf";
        var responseStream = new MemoryStream([1, 2, 3]);
        _s3.Setup(s => s.GetObjectAsync(BucketName, fileKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = responseStream });

        await _sut.DownloadAsync(fileKey);

        _s3.Verify(s => s.GetObjectAsync(BucketName, fileKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DownloadAsync_ValidKey_ReturnsResponseStream()
    {
        var responseStream = new MemoryStream([4, 5, 6]);
        _s3.Setup(s => s.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = responseStream });

        Stream result = await _sut.DownloadAsync("courses/abc/doc.pdf");

        result.ShouldBeSameAs(responseStream);
    }

    [Fact]
    public async Task DownloadAsync_S3Throws_PropagatesException()
    {
        _s3.Setup(s => s.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("Not found"));

        await Should.ThrowAsync<AmazonS3Exception>(() => _sut.DownloadAsync("courses/missing/doc.pdf"));
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ValidKey_CallsDeleteObjectWithCorrectBucketAndKey()
    {
        const string fileKey = "courses/abc/doc.pdf";
        _s3.Setup(s => s.DeleteObjectAsync(BucketName, fileKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteObjectResponse());

        await _sut.DeleteAsync(fileKey);

        _s3.Verify(s => s.DeleteObjectAsync(BucketName, fileKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_S3Throws_PropagatesException()
    {
        _s3.Setup(s => s.DeleteObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("Delete failed"));

        await Should.ThrowAsync<AmazonS3Exception>(() => _sut.DeleteAsync("courses/abc/doc.pdf"));
    }

    [Fact]
    public async Task DeleteAsync_CalledOnce_DoesNotCallUploadOrDownload()
    {
        _s3.Setup(s => s.DeleteObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteObjectResponse());

        await _sut.DeleteAsync("courses/abc/doc.pdf");

        _s3.Verify(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _s3.Verify(s => s.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
