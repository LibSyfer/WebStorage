using WebStorage.Domain.Common;

namespace WebStorage.Domain.Entities;

public class FileEntry : Entity
{
    public string UserId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; }
}
