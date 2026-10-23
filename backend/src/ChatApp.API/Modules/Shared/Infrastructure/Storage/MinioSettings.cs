using System.ComponentModel.DataAnnotations;

namespace ChatApp.API.Modules.Shared.Infrastructure.Storage;

public sealed class MinioSettings
{
    public const string SectionName = "Minio";

    [Required(AllowEmptyStrings = false)]
    public string Endpoint { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string AccessKey { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string SecretKey { get; set; } = string.Empty;

    public bool UseSSL { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string BucketName { get; set; } = "chat-attachments";

    [Range(1, 24 * 60)]
    public int PresignedUrlExpiryMinutes { get; set; } = 60;

    [Range(1, long.MaxValue)]
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
}