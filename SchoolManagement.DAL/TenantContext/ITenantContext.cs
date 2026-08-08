namespace SchoolManagement.DAL.TenantContext;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
    string? SchemaName { get; }
    string? TenantName { get; }
    bool IsResolved { get; }
    bool IsSuperAdmin { get; }
    void SetTenant(Guid tenantId, string slug, string schemaName, string name);
    void SetSuperAdmin();
    void Clear();
}

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public string? TenantSlug { get; private set; }
    public string? SchemaName { get; private set; }
    public string? TenantName { get; private set; }
    public bool IsResolved => !string.IsNullOrEmpty(SchemaName) || IsSuperAdmin;
    public bool IsSuperAdmin { get; private set; }

    public void SetTenant(Guid tenantId, string slug, string schemaName, string name)
    {
        TenantId = tenantId;
        TenantSlug = slug;
        SchemaName = schemaName;
        TenantName = name;
        IsSuperAdmin = false;
    }

    public void SetSuperAdmin()
    {
        IsSuperAdmin = true;
        TenantId = null;
        TenantSlug = null;
        SchemaName = null;
        TenantName = null;
    }

    public void Clear()
    {
        TenantId = null;
        TenantSlug = null;
        SchemaName = null;
        TenantName = null;
        IsSuperAdmin = false;
    }
}
