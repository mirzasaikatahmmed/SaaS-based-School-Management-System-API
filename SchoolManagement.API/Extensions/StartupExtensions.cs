using Microsoft.EntityFrameworkCore;
using SchoolManagement.BLL.Helpers;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.BLL.Settings;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.API.Extensions;

public static class StartupExtensions
{
    public static async Task InitializeApplicationAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");

        var masterDb = scope.ServiceProvider.GetRequiredService<MasterDbContext>();

        logger.LogInformation("Applying MasterDbContext migrations...");
        await masterDb.Database.MigrateAsync();

        await SeedSuperAdminAsync(scope.ServiceProvider, logger);

        var storage = scope.ServiceProvider.GetRequiredService<IStorageService>();
        await storage.VerifyConnectionAsync();

        logger.LogInformation("Application initialization complete.");
    }

    private static async Task SeedSuperAdminAsync(IServiceProvider services, ILogger logger)
    {
        var tenantRepo = services.GetRequiredService<ITenantRepository>();
        var settings = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SuperAdminSettings>>().Value;
        var masterDb = services.GetRequiredService<MasterDbContext>();

        if (await tenantRepo.SuperAdminExistsAsync())
        {
            logger.LogInformation("Super admin already exists — skipping seed.");
            return;
        }

        var admin = new SuperAdmin
        {
            Id = Guid.NewGuid(),
            Email = settings.Email.ToLowerInvariant(),
            Username = settings.Username.ToLowerInvariant(),
            PasswordHash = PasswordHelper.HashPassword(settings.Password),
            FirstName = settings.FirstName,
            LastName = settings.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await tenantRepo.AddSuperAdminAsync(admin);
        await masterDb.SaveChangesAsync();

        logger.LogInformation("Seeded super admin {Email} with role {Role}",
            admin.Email, AppConstants.Roles.SuperAdmin);
    }
}
