using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Entities.Master;

namespace SchoolManagement.DAL.Context;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<SuperAdmin> SuperAdmins => Set<SuperAdmin>();
    public DbSet<GlobalSettings> GlobalSettings => Set<GlobalSettings>();
    public DbSet<BiometricDeviceRegistry> BiometricDeviceRegistries => Set<BiometricDeviceRegistry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Domain).HasColumnName("domain").HasMaxLength(255);
            entity.Property(e => e.SchemaName).HasColumnName("schema_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.SubscriptionPlan).HasColumnName("subscription_plan").HasMaxLength(50).HasDefaultValue("basic");
            entity.Property(e => e.MaxUsers).HasColumnName("max_users").HasDefaultValue(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.Settings).HasColumnName("settings").HasColumnType("jsonb").HasDefaultValue("{}");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.Website).HasColumnName("website").HasMaxLength(255);
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
            entity.Property(e => e.State).HasColumnName("state").HasMaxLength(100);
            entity.Property(e => e.Country).HasColumnName("country").HasMaxLength(100).HasDefaultValue("Bangladesh");
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(10).HasDefaultValue("BDT");
            entity.Property(e => e.CurrencySymbol).HasColumnName("currency_symbol").HasMaxLength(10).HasDefaultValue("৳");
            entity.Property(e => e.Timezone).HasColumnName("timezone").HasMaxLength(100).HasDefaultValue("Asia/Dhaka");
            entity.Property(e => e.Locale).HasColumnName("locale").HasMaxLength(20).HasDefaultValue("en-US");
            entity.Property(e => e.LogoUrl).HasColumnName("logo_url").HasMaxLength(500);
            entity.Property(e => e.EstablishedYear).HasColumnName("established_year");
            entity.Property(e => e.SchoolType).HasColumnName("school_type").HasMaxLength(50);
            entity.Property(e => e.SubscriptionExpiresAt).HasColumnName("subscription_expires_at");

            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Domain).IsUnique();
            entity.HasIndex(e => e.SchemaName).IsUnique();
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.State);
            entity.HasIndex(e => e.SchoolType);
            entity.HasIndex(e => e.IsActive);
        });

        // School inherits Tenant — ignore as separate entity to avoid TPH conflict
        modelBuilder.Ignore<School>();

        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.ToTable("super_admins");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        modelBuilder.Entity<GlobalSettings>(entity =>
        {
            entity.ToTable("global_settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SiteName).HasColumnName("site_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.SiteTitle).HasColumnName("site_title").HasMaxLength(200);
            entity.Property(e => e.SiteLogoUrl).HasColumnName("site_logo_url").HasMaxLength(500);
            entity.Property(e => e.SiteFaviconUrl).HasColumnName("site_favicon_url").HasMaxLength(500);
            entity.Property(e => e.AdminEmail).HasColumnName("admin_email").HasMaxLength(255);
            entity.Property(e => e.SupportPhone).HasColumnName("support_phone").HasMaxLength(50);
            entity.Property(e => e.DefaultTimezone).HasColumnName("default_timezone").HasMaxLength(100).HasDefaultValue("Asia/Dhaka");
            entity.Property(e => e.DefaultCurrency).HasColumnName("default_currency").HasMaxLength(10).HasDefaultValue("BDT");
            entity.Property(e => e.DefaultCurrencySymbol).HasColumnName("default_currency_symbol").HasMaxLength(10).HasDefaultValue("৳");
            entity.Property(e => e.DefaultLocale).HasColumnName("default_locale").HasMaxLength(20).HasDefaultValue("en-US");
            entity.Property(e => e.DefaultDateFormat).HasColumnName("default_date_format").HasMaxLength(20).HasDefaultValue("DD/MM/YYYY");
            entity.Property(e => e.MaintenanceMode).HasColumnName("maintenance_mode").HasDefaultValue(false);
            entity.Property(e => e.MaintenanceMessage).HasColumnName("maintenance_message");
            entity.Property(e => e.MaxUploadSizeMb).HasColumnName("max_upload_size_mb").HasDefaultValue(5);
            entity.Property(e => e.AllowedFileTypes).HasColumnName("allowed_file_types").HasMaxLength(500)
                .HasDefaultValue("jpg,jpeg,png,gif,pdf,doc,docx,xls,xlsx,csv");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<BiometricDeviceRegistry>(entity =>
        {
            entity.ToTable("biometric_device_registry");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SerialNumber).HasColumnName("serial_number").HasMaxLength(100).IsRequired();
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.SchemaName).HasColumnName("schema_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.DeviceName).HasColumnName("device_name").HasMaxLength(200);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.AttLogStamp).HasColumnName("att_log_stamp").HasMaxLength(50).HasDefaultValue("0");
            entity.Property(e => e.OperLogStamp).HasColumnName("oper_log_stamp").HasMaxLength(50).HasDefaultValue("0");
            entity.Property(e => e.LastSeenAt).HasColumnName("last_seen_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.SerialNumber).IsUnique();
            entity.HasIndex(e => e.TenantId);
            entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
