using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;

namespace SchoolManagement.DAL.Context;

public class TenantDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly string? _schemaOverride;

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        string schemaName) : base(options)
    {
        _tenantContext = new TenantContext.TenantContext();
        _schemaOverride = schemaName;
    }

    public string SchemaName =>
        _schemaOverride
        ?? _tenantContext.SchemaName
        ?? throw new InvalidOperationException("Tenant schema has not been resolved.");

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();
    public DbSet<ClassEntity> Classes => Set<ClassEntity>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<StudentCategory> StudentCategories => Set<StudentCategory>();
    public DbSet<TransportRoute> TransportRoutes => Set<TransportRoute>();
    public DbSet<Hostel> Hostels => Set<Hostel>();
    public DbSet<HostelRoom> HostelRooms => Set<HostelRoom>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Guardian> Guardians => Set<Guardian>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var schema = SchemaName;
        modelBuilder.HasDefaultSchema(schema);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Password).HasColumnName("password").HasMaxLength(250).IsRequired();
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Mobileno).HasColumnName("mobileno").HasMaxLength(100);
            entity.Property(e => e.Photo).HasColumnName("photo").HasMaxLength(255);
            entity.Property(e => e.Active).HasColumnName("active").HasDefaultValue(true);
            entity.Property(e => e.IsEmailVerified).HasColumnName("is_email_verified").HasDefaultValue(false);
            entity.Property(e => e.LastLogin).HasColumnName("last_login");
            entity.Property(e => e.FailedLoginAttempts).HasColumnName("failed_login_attempts").HasDefaultValue(0);
            entity.Property(e => e.LockoutEndAt).HasColumnName("lockout_end_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Prefix).HasColumnName("prefix").HasMaxLength(50).IsRequired();
            entity.Property(e => e.IsSystem).HasColumnName("is_system").HasDefaultValue(true);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Prefix).IsUnique();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles", schema);
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IsRevoked).HasColumnName("is_revoked").HasDefaultValue(false);
            entity.Property(e => e.CreatedByIp).HasColumnName("created_by_ip").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.ReplacedByToken).HasColumnName("replaced_by_token").HasMaxLength(500);
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsActive);

            entity.HasIndex(e => e.Token).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoginLog>(entity =>
        {
            entity.ToTable("login_log", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Ip).HasColumnName("ip").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Browser).HasColumnName("browser").HasMaxLength(255);
            entity.Property(e => e.Platform).HasColumnName("platform").HasMaxLength(255);
            entity.Property(e => e.Timestamp).HasColumnName("timestamp").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.LoginLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClassEntity>(entity =>
        {
            entity.ToTable("classes", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.NumericName).HasColumnName("numeric_name");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.ToTable("sections", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Class)
                .WithMany(c => c.Sections)
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudentCategory>(entity =>
        {
            entity.ToTable("student_categories", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<TransportRoute>(entity =>
        {
            entity.ToTable("transport_routes", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Hostel>(entity =>
        {
            entity.ToTable("hostels", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<HostelRoom>(entity =>
        {
            entity.ToTable("hostel_rooms", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.HostelId).HasColumnName("hostel_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Hostel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(e => e.HostelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("students", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RegisterNo).HasColumnName("register_no").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Roll).HasColumnName("roll").HasMaxLength(50);
            entity.Property(e => e.AcademicYear).HasColumnName("academic_year");
            entity.Property(e => e.AdmissionDate).HasColumnName("admission_date").HasColumnType("date");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100);
            entity.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(20);
            entity.Property(e => e.BloodGroup).HasColumnName("blood_group").HasMaxLength(10);
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date");
            entity.Property(e => e.MotherTongue).HasColumnName("mother_tongue").HasMaxLength(100);
            entity.Property(e => e.Religion).HasColumnName("religion").HasMaxLength(100);
            entity.Property(e => e.Caste).HasColumnName("caste").HasMaxLength(100);
            entity.Property(e => e.MobileNo).HasColumnName("mobile_no").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
            entity.Property(e => e.State).HasColumnName("state").HasMaxLength(100);
            entity.Property(e => e.PresentAddress).HasColumnName("present_address");
            entity.Property(e => e.PermanentAddress).HasColumnName("permanent_address");
            entity.Property(e => e.ProfilePictureUrl).HasColumnName("profile_picture_url").HasMaxLength(500);
            entity.Property(e => e.FathersNidNumber).HasColumnName("fathers_nid_number").HasMaxLength(100);
            entity.Property(e => e.MothersNidNumber).HasColumnName("mothers_nid_number").HasMaxLength(100);
            entity.Property(e => e.BirthRegistrationNumber).HasColumnName("birth_registration_number").HasMaxLength(100);
            entity.Property(e => e.PreviousSchoolName).HasColumnName("previous_school_name").HasMaxLength(255);
            entity.Property(e => e.PreviousSchoolQualification).HasColumnName("previous_school_qualification").HasMaxLength(255);
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.TransportRouteId).HasColumnName("transport_route_id");
            entity.Property(e => e.VehicleNo).HasColumnName("vehicle_no").HasMaxLength(50);
            entity.Property(e => e.HostelId).HasColumnName("hostel_id");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.RegisterNo).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Section)
                .WithMany(s => s.Students)
                .HasForeignKey(e => e.SectionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Students)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TransportRoute)
                .WithMany(t => t.Students)
                .HasForeignKey(e => e.TransportRouteId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Hostel)
                .WithMany(h => h.Students)
                .HasForeignKey(e => e.HostelId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Room)
                .WithMany(r => r.Students)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Guardian>(entity =>
        {
            entity.ToTable("guardians", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Relation).HasColumnName("relation").HasMaxLength(100).IsRequired();
            entity.Property(e => e.FatherName).HasColumnName("father_name").HasMaxLength(200);
            entity.Property(e => e.MotherName).HasColumnName("mother_name").HasMaxLength(200);
            entity.Property(e => e.Occupation).HasColumnName("occupation").HasMaxLength(200);
            entity.Property(e => e.Income).HasColumnName("income").HasColumnType("numeric(12,2)");
            entity.Property(e => e.Education).HasColumnName("education").HasMaxLength(200);
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
            entity.Property(e => e.State).HasColumnName("state").HasMaxLength(100);
            entity.Property(e => e.MobileNo).HasColumnName("mobile_no").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.ProfilePictureUrl).HasColumnName("profile_picture_url").HasMaxLength(500);
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Guardians)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
