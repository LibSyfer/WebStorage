using WebStorage.Domain.Entities;

namespace WebStorage.Application.Abstractions;

public interface IFileEntryRepository : IRepository<FileEntry>
{
    Task<IReadOnlyList<FileEntry>> GetAllByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<FileEntry?> GetByIdForUserAsync(Guid id, string userId, CancellationToken cancellationToken = default);
}
