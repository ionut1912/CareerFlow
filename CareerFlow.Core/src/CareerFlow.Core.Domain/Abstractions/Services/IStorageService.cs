namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName,
        string contentType, CancellationToken ct = default);

    Task<Stream> DownloadAsync(string fileKey, CancellationToken ct = default);
    Task DeleteAsync(string fileKey, CancellationToken ct = default);
}