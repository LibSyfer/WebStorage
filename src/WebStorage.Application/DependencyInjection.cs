using Microsoft.Extensions.DependencyInjection;
using WebStorage.Application.Files;

namespace WebStorage.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFileService, FileService>();
        return services;
    }
}
