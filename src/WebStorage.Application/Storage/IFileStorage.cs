namespace WebStorage.Application.Storage;

public interface IFileStorage
{
    Task SaveAsync(string storageKey, Stream file, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
