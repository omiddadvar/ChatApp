namespace ChatApp.API.Modules.Shared.Infrastructure.Storage;

public sealed class MinioSettings
{
    public const string SectionName = "Minio";

    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool UseSSL { get; set; }
    public string BucketName { get; set; } = "chat-attachments";
    public int PresignedUrlExpiryMinutes { get; set; } = 60;
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
}