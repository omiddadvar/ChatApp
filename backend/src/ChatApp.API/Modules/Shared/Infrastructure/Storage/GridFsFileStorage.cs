using ChatApp.API.Modules.Shared.Infrastructure.Models;
using ChatApp.API.Modules.Shared.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace ChatApp.API.Modules.Shared.Infrastructure.Storage;

public sealed class GridFsFileStorage : IFileStorage
{
    private readonly IGridFSBucket _bucket;
    private readonly ILogger<GridFsFileStorage> _logger;

    public GridFsFileStorage(IMongoContext mongoContext, ILogger<GridFsFileStorage> logger)
    {
        _logger = logger;

        var options = new GridFSBucketOptions
        {
            BucketName = "attachments",
            ChunkSizeBytes = 255 * 1024 // 255 KB
        };

        _bucket = new GridFSBucket(mongoContext.Database, options);
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        long size,
        CancellationToken ct = default)
    {
        var uploadOptions = new GridFSUploadOptions
        {
            Metadata = new BsonDocument
            {
                { "contentType", contentType },
                { "uploadedAt", DateTime.UtcNow }
            }
        };

        var fileId = await _bucket.UploadFromStreamAsync(
            fileName, stream, uploadOptions, ct);

        _logger.LogInformation(
            "Uploaded {FileName} ({Size} bytes) to GridFS with id {FileId}",
            fileName, size, fileId);

        return new FileUploadResult(
            FileId: fileId.ToString(),
            FileName: fileName,
            Size: size,
            ContentType: contentType);
    }

    public async Task<Stream> DownloadAsync(string fileId, CancellationToken ct = default)
    {
        var objectId = ObjectId.Parse(fileId);
        return await _bucket.OpenDownloadStreamAsync(objectId, cancellationToken: ct);
    }

    public async Task<bool> DeleteAsync(string fileId, CancellationToken ct = default)
    {
        try
        {
            var objectId = ObjectId.Parse(fileId);
            await _bucket.DeleteAsync(objectId, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete GridFS file {FileId}", fileId);
            return false;
        }
    }

    public async Task<FileMetadata?> GetMetadataAsync(string fileId, CancellationToken ct = default)
    {
        var objectId = ObjectId.Parse(fileId);
        var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", objectId);

        using var cursor = await _bucket.FindAsync(filter, cancellationToken: ct);
        var fileInfo = await cursor.FirstOrDefaultAsync(ct);

        if (fileInfo is null) return null;

        var contentType = fileInfo.Metadata?
            .GetValue("contentType", "application/octet-stream")
            .AsString ?? "application/octet-stream";

        return new FileMetadata(
            FileId: fileInfo.Id.ToString(),
            FileName: fileInfo.Filename,
            ContentType: contentType,
            Size: fileInfo.Length,
            UploadedAt: fileInfo.UploadDateTime);
    }
}