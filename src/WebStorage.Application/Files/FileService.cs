using WebStorage.Application.Abstractions;
using WebStorage.Application.Storage;
using WebStorage.Domain.Entities;

namespace WebStorage.Application.Files;

public sealed class FileService(
    IFileStorage fileStorage,
    IFileEntryRepository fileEntries,
    IUserStorageRepository userStorages,
    IUnitOfWork uow) : IFileService
{
    private const long DefaultMaxBytes = 1024L * 1024L * 1024L;

    public async Task<Guid> UploadAsync(
        string userId,
        string fileName,
        Stream content,
        long contentLength,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("FileName is required.", nameof(fileName));
        if (contentLength <= 0) throw new ArgumentOutOfRangeException(nameof(contentLength), "Content length must be > 0.");

        var userStorage = await userStorages.GetOrCreateAsync(userId, DefaultMaxBytes, cancellationToken);
        if (!userStorage.TryReserve(contentLength))
            throw new InvalidOperationException("Storage quota exceeded.");

        var fileId = Guid.NewGuid();
        var storageKey = BuildStorageKey(userId, fileId, fileName);

        try
        {
            await fileStorage.SaveAsync(storageKey, content, cancellationToken);

            var entry = new FileEntry
            {
                Id = fileId,
                UserId = userId,
                FileName = fileName,
                StorageKey = storageKey,
                Size = contentLength,
                UploadedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await fileEntries.AddAsync(entry, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);

            return fileId;
        }
        catch
        {
            userStorage.TryRelease(contentLength);
            throw;
        }
    }

    public async Task<IReadOnlyList<FileEntryDto>> ListAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entries = await fileEntries.GetAllByUserAsync(userId, cancellationToken);
        return entries.Select(e => e.ToDto()).ToList();
    }

    public async Task<(Stream Stream, string FileName)> OpenReadAsync(Guid fileId, string userId, CancellationToken cancellationToken = default)
    {
        var entry = await fileEntries.GetByIdForUserAsync(fileId, userId, cancellationToken);
        if (entry is null) throw new InvalidOperationException("File not found.");

        var stream = await fileStorage.OpenReadAsync(entry.StorageKey, cancellationToken);
        return (stream, entry.FileName);
    }

    public async Task<bool> DeleteAsync(Guid fileId, string userId, CancellationToken cancellationToken = default)
    {
        var entry = await fileEntries.GetByIdForUserAsync(fileId, userId, cancellationToken);
        if (entry is null) return false;

        await fileStorage.DeleteAsync(entry.StorageKey, cancellationToken);
        var deleted = await fileEntries.DeleteAsync(entry.Id, cancellationToken);

        var userStorage = await userStorages.GetByUserIdAsync(userId, cancellationToken);
        userStorage?.TryRelease(entry.Size);

        await uow.SaveChangesAsync(cancellationToken);
        return deleted;
    }

    private static string BuildStorageKey(string userId, Guid fileId, string fileName)
    {
        var safeName = string.Concat(fileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        safeName = string.IsNullOrWhiteSpace(safeName) ? "file" : safeName;
        return $"{userId}/{fileId:N}/{safeName}";
    }
}

