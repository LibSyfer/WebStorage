namespace WebStorage.Application.StorageAccounts;

public sealed record UserStorageDto(string UserId, long UsedBytes, long MaxBytes);

