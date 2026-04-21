using WebStorage.Application.Common;

namespace WebStorage.Application.Storage;

public sealed class StorageKeyInvalidException()
    : AppErrorException("The storage key is invalid.")
{
    public override int StatusCode => 400;

    public override string Title => "Invalid storage key";

    public override string? ErrorCode => ErrorCodes.Storage.InvalidKey;
}

public sealed class StorageQuotaExceededException()
    : AppErrorException("Not enough free storage to upload this file.")
{
    public override int StatusCode => 413;

    public override string Title => "Storage quota exceeded";

    public override string? ErrorCode => ErrorCodes.Storage.QuotaExceeded;
}

public sealed class StoredFileNotFoundException(Guid fileId)
    : AppErrorException("The requested file was not found.")
{
    public Guid FileId { get; } = fileId;

    public override int StatusCode => 404;

    public override string Title => "File not found";

    public override string? ErrorCode => ErrorCodes.Storage.FileNotFound;
}
