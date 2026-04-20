using WebStorage.Application.Abstractions;

namespace WebStorage.Application.StorageAccounts;

public sealed class StorageAccountService(
    IUserStorageRepository userStorages,
    IUnitOfWork uow) : IStorageAccountService
{
    private const long DefaultMaxBytes = 1024L * 1024L * 1024L; // 1 GiB (can be moved to options later)

    public async Task<UserStorageDto> GetOrCreateMyAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId is required.", nameof(userId));

        var storage = await userStorages.GetOrCreateAsync(userId, DefaultMaxBytes, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return new UserStorageDto(storage.UserId, storage.UsedBytes, storage.MaxBytes);
    }

    public async Task<UserStorageDto> SetQuotaAsync(string userId, long newMaxBytes, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId is required.", nameof(userId));
        if (newMaxBytes < 0) newMaxBytes = 0;

        var storage = await userStorages.GetOrCreateAsync(userId, DefaultMaxBytes, cancellationToken);
        storage.ChangeQuota(newMaxBytes);

        await uow.SaveChangesAsync(cancellationToken);
        return new UserStorageDto(storage.UserId, storage.UsedBytes, storage.MaxBytes);
    }
}

