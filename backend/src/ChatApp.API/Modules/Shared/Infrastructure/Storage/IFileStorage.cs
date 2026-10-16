namespace ChatApp.API.Modules.Shared.Infrastructure.Storage;

public interface IFileStorage
{
    Task<FileUploadResult> UploadAsync(
        Stream stream,
        string objectName,
        string contentType,
        long size,
        CancellationToken ct = default);

    Task<Stream> DownloadAsync(string objectName, CancellationToken ct = default);
    Task<bool> DeleteAsync(string objectName, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string objectName, CancellationToken ct = default);
    Task<bool> ExistsAsync(string objectName, CancellationToken ct = default);
}

public sealed record FileUploadResult(
    string ObjectName,
    string Bucket,
    long Size,
    string ContentType,
    string? ETag);