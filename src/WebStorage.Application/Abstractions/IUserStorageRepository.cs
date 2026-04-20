using WebStorage.Domain.Entities;

namespace WebStorage.Application.Abstractions;

public interface IUserStorageRepository : IRepository<UserStorage>
{
    Task<UserStorage?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserStorage> GetOrCreateAsync(string userId, long defaultMaxBytes, CancellationToken cancellationToken = default);
}