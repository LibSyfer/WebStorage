using WebStorage.Domain.Common;

namespace WebStorage.Domain.Entities;

public class UserStorage : Entity
{
    public string UserId { get; set; } = string.Empty;
    public long UsedBytes { get; set; }
    public long MaxBytes { get; set; }

    public bool TryReserve(long bytes)
    {
        if (UsedBytes + bytes > MaxBytes) return false;

        UsedBytes += bytes;
        return true;
    }

    public bool TryRelease(long bytes)
    {
        if (UsedBytes - bytes < 0) return false;

        UsedBytes -= bytes;
        return true;
    }

    public void ChangeQuota(long newQuota)
    {
        MaxBytes = newQuota;
        if (MaxBytes < 0) MaxBytes = 0;
    }
}
