namespace ChatApp.API.Modules.Shared.Infrastructure.Models;

public sealed record FileMetadata(
    string FileId,
    string FileName,
    string ContentType,
    long Size,
    DateTimeOffset UploadedAt);