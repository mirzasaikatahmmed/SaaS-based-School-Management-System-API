using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SchoolManagement.DAL.Context;

public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("MasterDb")
            ?? "Host=localhost;Port=5432;Database=school_master;Username=schooladmin;Password=schoolpassword";

        var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
            npgsql.MigrationsAssembly(typeof(MasterDbContext).Assembly.FullName);
        });

        return new MasterDbContext(optionsBuilder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SchoolManagement.API");
        if (!Directory.Exists(basePath))
            basePath = Directory.GetCurrentDirectory();

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
    }
}

public class TenantDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("MasterDb")
            ?? "Host=localhost;Port=5432;Database=school_master;Username=schooladmin;Password=schoolpassword";

        var schema = args.Length > 0 ? args[0] : "tenant_template";

        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema);
            npgsql.MigrationsAssembly(typeof(TenantDbContext).Assembly.FullName);
        });

        return new TenantDbContext(optionsBuilder.Options, schema);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SchoolManagement.API");
        if (!Directory.Exists(basePath))
            basePath = Directory.GetCurrentDirectory();

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
    }
}
