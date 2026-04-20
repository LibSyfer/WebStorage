using Microsoft.Extensions.DependencyInjection;

namespace WebStorage.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
