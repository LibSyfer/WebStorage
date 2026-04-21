using Microsoft.Extensions.Options;
using WebStorage.Application.Storage;
using WebStorage.Infrastructure.Options;

namespace WebStorage.Infrastructure.Storage;

public sealed class LocalFileStorage(IOptions<FileStorageOptions> options) : IFileStorage
{
    private readonly string _rootPath = Path.GetFullPath(options.Value.RootPath);

    public async Task SaveAsync(string storageKey, Stream file, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var target = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 64 * 1024, useAsync: true);
        await file.CopyToAsync(target, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(storageKey);
        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 64 * 1024, useAsync: true);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(storageKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string ResolvePath(string storageKey)
    {
        var relative = storageKey.Replace('\\', '/').TrimStart('/');
        var combined = Path.Combine(_rootPath, relative.Replace('/', Path.DirectorySeparatorChar));
        var full = Path.GetFullPath(combined);

        if (!full.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
            throw new StorageKeyInvalidException();

        return full;
    }
}

