using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebStorage.Application.Auth;
using WebStorage.Infrastructure.Identity;

namespace WebStorage.Infrastructure.Data;

public static class DbSeeder
{
    private static readonly string[] Roles = [RoleNames.User, RoleNames.Admin];

    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        await ExecuteMigrationsAsync(sp, cancellationToken);
        await SeedRolesAsync(sp, cancellationToken);
        await SeedAdminAsync(sp, configuration, cancellationToken);
    }

    private static async Task ExecuteMigrationsAsync(IServiceProvider sp, CancellationToken cancellationToken)
    {
        var logger = sp.GetRequiredService<LoggerFactory>().CreateLogger("MigrationExecutor");

        logger.LogInformation("Starting database migrations...");

        await sp.GetRequiredService<AppDbContext>().Database.MigrateAsync(cancellationToken);

        logger.LogInformation("Database migrations completed successfully");
    }

    private static async Task SeedRolesAsync(IServiceProvider sp, CancellationToken cancellationTokens)
    {
        var logger = sp.GetRequiredService<LoggerFactory>().CreateLogger("RolesSeeder");

        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                    logger.LogError("Failed to create role '{Role}': {Errors}", role, string.Join(", ", result.Errors.Select(e => e.Description)));

                logger.LogInformation("Created role '{Role}'", role);
            }
        }
    }

    private static async Task SeedAdminAsync(IServiceProvider sp, IConfiguration configuration, CancellationToken cancellationToken)
    {
        var logger = sp.GetRequiredService<LoggerFactory>().CreateLogger("AdminSeeder");

        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("Admin credentials not provided in configuration. Skipping admin seeding");
            return;
        }

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            logger.LogInformation("Admin user already exists. Skipping admin seeding");
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, RoleNames.Admin);

        logger.LogInformation("Admin user created successfully");
    }
}
