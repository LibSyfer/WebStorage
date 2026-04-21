using Microsoft.EntityFrameworkCore;
using WebStorage.Application.Abstractions;
using WebStorage.Domain.Entities;

namespace WebStorage.Infrastructure.Data.Repositories;

public sealed class FileEntryRepository(AppDbContext dbContext) : IFileEntryRepository
{
    public async Task<FileEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.FileEntries.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<FileEntry?> GetByIdForUserAsync(Guid id, string userId, CancellationToken cancellationToken = default) =>
        await dbContext.FileEntries.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<FileEntry>> GetAllByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await dbContext.FileEntries
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync(cancellationToken);

    public async Task<FileEntry> AddAsync(FileEntry entity, CancellationToken cancellationToken = default)
    {
        await dbContext.FileEntries.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await dbContext.FileEntries.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entry is null) return false;
        dbContext.FileEntries.Remove(entry);
        return true;
    }
}

