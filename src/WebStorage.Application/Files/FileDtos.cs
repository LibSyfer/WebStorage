using WebStorage.Domain.Entities;

namespace WebStorage.Application.Files;

public sealed record FileEntryDto(
    Guid Id,
    string FileName,
    long Size,
    DateTime UploadedAt);

public static class FileEntryMappings
{
    public static FileEntryDto ToDto(this FileEntry entry) =>
        new(entry.Id, entry.FileName, entry.Size, entry.UploadedAt);
}

