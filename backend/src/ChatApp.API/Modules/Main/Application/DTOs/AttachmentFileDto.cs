namespace ChatApp.API.Modules.Main.Application.DTOs;

public record AttachmentFileDto(
    Stream Stream,
    string FileName,
    string ContentType);