using ChatApp.API.Modules.Shared.Infrastructure.Models;

namespace ChatApp.API.Modules.Shared.Infrastructure.Storage;

public interface IFileStorage
{
    Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        long size,
        CancellationToken ct = default);

    Task<Stream> DownloadAsync(string fileId, CancellationToken ct = default);

    Task<bool> DeleteAsync(string fileId, CancellationToken ct = default);

    Task<FileMetadata?> GetMetadataAsync(string fileId, CancellationToken ct = default);
}