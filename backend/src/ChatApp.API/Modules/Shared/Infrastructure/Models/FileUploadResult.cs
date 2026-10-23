namespace ChatApp.API.Modules.Shared.Infrastructure.Models;

public sealed record FileUploadResult(
    string FileId,
    string FileName,
    long Size,
    string ContentType);
