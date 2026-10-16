using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace ChatApp.API.Modules.Shared.Infrastructure.Storage;

public sealed class MinioFileStorage : IFileStorage
{
    private readonly IMinioClient _client;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioFileStorage> _logger;
    private bool _bucketChecked;

    public MinioFileStorage(
        IMinioClient client,
        IOptions<MinioSettings> settings,
        ILogger<MinioFileStorage> logger)
    {
        _client = client;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream stream,
        string objectName,
        string contentType,
        long size,
        CancellationToken ct = default)
    {
        if (size > _settings.MaxFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"File exceeds maximum allowed size of {_settings.MaxFileSizeBytes} bytes.");
        }

        await EnsureBucketAsync(ct);

        var args = new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(size)
            .WithContentType(contentType);

        var response = await _client.PutObjectAsync(args, ct).ConfigureAwait(false);

        _logger.LogInformation(
            "Uploaded {ObjectName} ({Size} bytes) to MinIO",
            objectName,
            size);

        return new FileUploadResult(
            objectName,
            _settings.BucketName,
            size,
            contentType,
            response.Etag);
    }

    public async Task<Stream> DownloadAsync(string objectName, CancellationToken ct = default)
    {
        var ms = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithCallbackStream(s => s.CopyTo(ms));

        await _client.GetObjectAsync(args, ct).ConfigureAwait(false);
        ms.Position = 0;
        return ms;
    }

    public async Task<bool> DeleteAsync(string objectName, CancellationToken ct = default)
    {
        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName);

            await _client.RemoveObjectAsync(args, ct).ConfigureAwait(false);
            return true;
        }
        catch (MinioException ex)
        {
            _logger.LogError(ex, "Failed to delete {ObjectName}", objectName);
            return false;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, CancellationToken ct = default)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithExpiry(_settings.PresignedUrlExpiryMinutes * 60);

        return await _client.PresignedGetObjectAsync(args).ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(string objectName, CancellationToken ct = default)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName);

            await _client.StatObjectAsync(args, ct).ConfigureAwait(false);
            return true;
        }
        catch (MinioException)
        {
            return false;
        }
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        if (_bucketChecked) return;

        var existsArgs = new BucketExistsArgs().WithBucket(_settings.BucketName);
        var exists = await _client.BucketExistsAsync(existsArgs, ct).ConfigureAwait(false);

        if (!exists)
        {
            var makeArgs = new MakeBucketArgs().WithBucket(_settings.BucketName);
            await _client.MakeBucketAsync(makeArgs, ct).ConfigureAwait(false);
            _logger.LogInformation("Created MinIO bucket {Bucket}", _settings.BucketName);
        }

        _bucketChecked = true;
    }
}