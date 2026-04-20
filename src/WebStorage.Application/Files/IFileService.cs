namespace WebStorage.Application.Files;

public interface IFileService
{
    Task<Guid> UploadAsync(
        string userId,
        string fileName,
        Stream content,
        long contentLength,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileEntryDto>> ListAsync(string userId, CancellationToken cancellationToken = default);

    Task<(Stream Stream, string FileName)> OpenReadAsync(Guid fileId, string userId, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid fileId, string userId, CancellationToken cancellationToken = default);
}

