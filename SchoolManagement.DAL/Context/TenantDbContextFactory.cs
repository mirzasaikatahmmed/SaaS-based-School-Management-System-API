using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using SchoolManagement.DAL.TenantContext;

namespace SchoolManagement.DAL.Context;

public interface ITenantDbContextFactory
{
    TenantDbContext Create();
    TenantDbContext Create(string schemaName);
}

public class TenantDbContextFactory : ITenantDbContextFactory
{
    private readonly IConfiguration _configuration;
    private readonly ITenantContext _tenantContext;

    public TenantDbContextFactory(IConfiguration configuration, ITenantContext tenantContext)
    {
        _configuration = configuration;
        _tenantContext = tenantContext;
    }

    public TenantDbContext Create()
    {
        if (string.IsNullOrEmpty(_tenantContext.SchemaName))
            throw new InvalidOperationException("Cannot create TenantDbContext: tenant schema is not resolved.");

        return Create(_tenantContext.SchemaName);
    }

    public TenantDbContext Create(string schemaName)
    {
        var connectionString = _configuration.GetConnectionString("MasterDb")
            ?? throw new InvalidOperationException("Connection string 'MasterDb' is missing.");

        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schemaName);
        });
        optionsBuilder.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();

        return new TenantDbContext(optionsBuilder.Options, schemaName);
    }
}
