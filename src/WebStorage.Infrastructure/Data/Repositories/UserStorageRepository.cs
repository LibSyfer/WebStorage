using Microsoft.EntityFrameworkCore;
using WebStorage.Application.Abstractions;
using WebStorage.Domain.Entities;

namespace WebStorage.Infrastructure.Data.Repositories;

public sealed class UserStorageRepository(AppDbContext dbContext) : IUserStorageRepository
{
    public async Task<UserStorage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.UserStorages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<UserStorage?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        await dbContext.UserStorages.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public async Task<UserStorage> GetOrCreateAsync(string userId, long defaultMaxBytes, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.UserStorages.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (existing is not null) return existing;

        var created = new UserStorage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedBytes = 0,
            MaxBytes = defaultMaxBytes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await dbContext.UserStorages.AddAsync(created, cancellationToken);
        return created;
    }

    public async Task<UserStorage> AddAsync(UserStorage entity, CancellationToken cancellationToken = default)
    {
        await dbContext.UserStorages.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.UserStorages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;
        dbContext.UserStorages.Remove(entity);
        return true;
    }
}

