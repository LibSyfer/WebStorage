using Microsoft.Extensions.DependencyInjection;
using WebStorage.Application.Storage;
using WebStorage.Application.StorageAccounts;

namespace WebStorage.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IStorageAccountService, StorageAccountService>();
        return services;
    }
}
