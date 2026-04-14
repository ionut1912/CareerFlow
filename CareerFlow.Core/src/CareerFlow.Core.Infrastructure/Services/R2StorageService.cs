using Amazon.S3;
using Amazon.S3.Model;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace CareerFlow.Core.Infrastructure.Services;

public sealed class R2StorageService : IStorageService
{
    private readonly string _bucket;
    private readonly IAmazonS3 _s3;

    public R2StorageService(IAmazonS3 s3, IOptions<R2Settings> options)
    {
        _s3 = s3;
        _bucket = options.Value.BucketName;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName,
        string contentType, CancellationToken ct = default)
    {
        var key = $"courses/{Guid.NewGuid()}/{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
            AutoCloseStream = false,
            DisablePayloadSigning = true, // 👈
            UseChunkEncoding = false // 👈
        };

        await _s3.PutObjectAsync(request, ct);
        return key;
    }

    public async Task<Stream> DownloadAsync(string fileKey, CancellationToken ct = default)
    {
        var response = await _s3.GetObjectAsync(_bucket, fileKey, ct);
        return response.ResponseStream;
    }

    public async Task DeleteAsync(string fileKey, CancellationToken ct = default)
    {
        await _s3.DeleteObjectAsync(_bucket, fileKey, ct);
    }
}