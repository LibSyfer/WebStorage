namespace WebStorage.Application.StorageAccounts;

public interface IStorageAccountService
{
    Task<UserStorageDto> GetOrCreateMyAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserStorageDto> SetQuotaAsync(string userId, long newMaxBytes, CancellationToken cancellationToken = default);
}

