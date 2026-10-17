using ChatApp.API.Modules.Shared.Kernel.Entities;
using ChatApp.API.Modules.Shared.Kernel.Exceptions;

namespace ChatApp.API.Modules.Main.Domain;

public class Attachment : AggregateRoot
{
    public Guid UploadedByUserId { get; private set; }
    public string ObjectName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }

    private Attachment() { }

    private const long MAX_FILE_SIZE = 10 * 1024 * 1024;
    private const string MAX_FILE_SIZE_STR = "10 MB";

    public static Attachment Create(
        Guid uploadedByUserId,
        string objectName,
        string originalFileName,
        string contentType,
        long sizeBytes)
    {
        if (sizeBytes <= 0)
            throw new DomainException("File size must be greater than zero.");

        if (sizeBytes > MAX_FILE_SIZE)
            throw new DomainException($"File must not exceed {MAX_FILE_SIZE_STR}.");

        return new Attachment
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = uploadedByUserId,
            ObjectName = objectName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            UploadedAt = DateTimeOffset.UtcNow
        };
    }
}