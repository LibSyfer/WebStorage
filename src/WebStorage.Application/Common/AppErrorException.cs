namespace WebStorage.Application.Common;

public abstract class AppErrorException(string message) : Exception(message)
{
    public abstract int StatusCode { get; }

    public abstract string Title { get; }

    public virtual string? ErrorCode => null;
}
