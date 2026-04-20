using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebStorage.Application.Abstractions;
using WebStorage.Application.Auth;
using WebStorage.Application.Storage;
using WebStorage.Infrastructure.Auth;
using WebStorage.Infrastructure.Data.Repositories;
using WebStorage.Infrastructure.Data;
using WebStorage.Infrastructure.Identity;
using WebStorage.Infrastructure.Options;
using WebStorage.Infrastructure.Storage;

namespace WebStorage.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AuthSessionOptions>(configuration.GetSection(AuthSessionOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        var dbConnectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dbConnectionString));

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFileEntryRepository, FileEntryRepository>();
        services.AddScoped<IUserStorageRepository, UserStorageRepository>();

        services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }
}
