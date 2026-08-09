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
    public DbSet<DeactivateReason> DeactivateReasons => Set<DeactivateReason>();
    public DbSet<TransportRoute> TransportRoutes => Set<TransportRoute>();
    public DbSet<Hostel> Hostels => Set<Hostel>();
    public DbSet<HostelRoom> HostelRooms => Set<HostelRoom>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<OnlineAdmission> OnlineAdmissions => Set<OnlineAdmission>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportBatchRow> ImportBatchRows => Set<ImportBatchRow>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeImportBatch> EmployeeImportBatches => Set<EmployeeImportBatch>();
    public DbSet<EmployeeImportBatchRow> EmployeeImportBatchRows => Set<EmployeeImportBatchRow>();
    public DbSet<SalaryTemplate> SalaryTemplates => Set<SalaryTemplate>();
    public DbSet<SalaryAllowance> SalaryAllowances => Set<SalaryAllowance>();
    public DbSet<SalaryDeduction> SalaryDeductions => Set<SalaryDeduction>();
    public DbSet<EmployeeSalaryAssignment> EmployeeSalaryAssignments => Set<EmployeeSalaryAssignment>();
    public DbSet<SalaryPayment> SalaryPayments => Set<SalaryPayment>();
    public DbSet<AdvanceSalaryRequest> AdvanceSalaryRequests => Set<AdvanceSalaryRequest>();
    public DbSet<LeaveCategory> LeaveCategories => Set<LeaveCategory>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Award> Awards => Set<Award>();
    public DbSet<ClassSection> ClassSections => Set<ClassSection>();
    public DbSet<ClassTeacherAllocation> ClassTeacherAllocations => Set<ClassTeacherAllocation>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ClassSubjectAssignment> ClassSubjectAssignments => Set<ClassSubjectAssignment>();
    public DbSet<ClassSubjectAssignmentItem> ClassSubjectAssignmentItems => Set<ClassSubjectAssignmentItem>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<ClassSchedulePeriod> ClassSchedulePeriods => Set<ClassSchedulePeriod>();
    public DbSet<StudentPromotion> StudentPromotions => Set<StudentPromotion>();
    public DbSet<ExamTerm> ExamTerms => Set<ExamTerm>();
    public DbSet<ExamHall> ExamHalls => Set<ExamHall>();
    public DbSet<MarkDistribution> MarkDistributions => Set<MarkDistribution>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamMarkDistribution> ExamMarkDistributions => Set<ExamMarkDistribution>();
    public DbSet<ExamSchedule> ExamSchedules => Set<ExamSchedule>();
    public DbSet<ExamScheduleSubject> ExamScheduleSubjects => Set<ExamScheduleSubject>();
    public DbSet<MarkEntry> MarkEntries => Set<MarkEntry>();

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
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Class)
                .WithMany(c => c.Sections)
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        modelBuilder.Entity<ClassSection>(entity =>
        {
            entity.ToTable("class_sections", schema);
            entity.HasKey(e => new { e.ClassId, e.SectionId });
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.HasOne(e => e.Class).WithMany(c => c.ClassSections).HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Section).WithMany(s => s.ClassSections).HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Cascade);
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

        modelBuilder.Entity<DeactivateReason>(entity =>
        {
            entity.ToTable("deactivate_reasons", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(200).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.Reason).IsUnique();
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
            entity.Property(e => e.DeactivateReason).HasColumnName("deactivate_reason");
            entity.Property(e => e.DeactivateReasonId).HasColumnName("deactivate_reason_id");
            entity.Property(e => e.DeactivatedAt).HasColumnName("deactivated_at");
            entity.Property(e => e.DeactivatedBy).HasColumnName("deactivated_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.RegisterNo).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeactivateReasonRef)
                .WithMany(r => r.Students)
                .HasForeignKey(e => e.DeactivateReasonId)
                .OnDelete(DeleteBehavior.SetNull);

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
            entity.Property(e => e.ReferenceNo).HasColumnName("reference_no").HasMaxLength(50);
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
            entity.Property(e => e.AlternativeParentName).HasColumnName("alternative_parent_name").HasMaxLength(200);
            entity.Property(e => e.AlternativeParentRelation).HasColumnName("alternative_parent_relation").HasMaxLength(100);
            entity.Property(e => e.AlternativeParentMobileNo).HasColumnName("alternative_parent_mobile").HasMaxLength(20);
            entity.Property(e => e.FacebookUrl).HasColumnName("facebook_url").HasMaxLength(500);
            entity.Property(e => e.TwitterUrl).HasColumnName("twitter_url").HasMaxLength(500);
            entity.Property(e => e.LinkedInUrl).HasColumnName("linkedin_url").HasMaxLength(500);
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary").HasDefaultValue(true);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.IsLoginActive).HasColumnName("is_login_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.ReferenceNo).IsUnique();

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Guardians)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OnlineAdmission>(entity =>
        {
            entity.ToTable("online_admissions", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ReferenceNo).HasColumnName("reference_no").HasMaxLength(50).IsRequired();
            entity.Property(e => e.AcademicYear).HasColumnName("academic_year");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.ClassName).HasColumnName("class_name").HasMaxLength(100);
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100);
            entity.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(20);
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date");
            entity.Property(e => e.BloodGroup).HasColumnName("blood_group").HasMaxLength(10);
            entity.Property(e => e.Religion).HasColumnName("religion").HasMaxLength(100);
            entity.Property(e => e.MobileNo).HasColumnName("mobile_no").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.PresentAddress).HasColumnName("present_address");
            entity.Property(e => e.PermanentAddress).HasColumnName("permanent_address");
            entity.Property(e => e.BirthRegistrationNumber).HasColumnName("birth_registration_number").HasMaxLength(100);
            entity.Property(e => e.ProfilePictureUrl).HasColumnName("profile_picture_url").HasMaxLength(500);
            entity.Property(e => e.GuardianName).HasColumnName("guardian_name").HasMaxLength(200);
            entity.Property(e => e.GuardianRelation).HasColumnName("guardian_relation").HasMaxLength(100);
            entity.Property(e => e.GuardianMobile).HasColumnName("guardian_mobile").HasMaxLength(20);
            entity.Property(e => e.GuardianEmail).HasColumnName("guardian_email").HasMaxLength(255);
            entity.Property(e => e.FatherName).HasColumnName("father_name").HasMaxLength(200);
            entity.Property(e => e.MotherName).HasColumnName("mother_name").HasMaxLength(200);
            entity.Property(e => e.PreviousSchoolName).HasColumnName("previous_school_name").HasMaxLength(255);
            entity.Property(e => e.PreviousSchoolQualification).HasColumnName("previous_school_qualification").HasMaxLength(255);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("Apply");
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status").HasMaxLength(20).IsRequired().HasDefaultValue("Unpaid");
            entity.Property(e => e.PaymentAmount).HasColumnName("payment_amount").HasColumnType("numeric(12,2)");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.PaymentReference).HasColumnName("payment_reference").HasMaxLength(200);
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.DeclineReason).HasColumnName("decline_reason");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.ApplyDate).HasColumnName("apply_date").HasDefaultValueSql("NOW()");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.ReferenceNo).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ClassId);

            entity.HasOne(e => e.Class)
                .WithMany()
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            // reviewed_by may be a Super Admin (public.super_admins) — no FK to tenant users

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ImportBatch>(entity =>
        {
            entity.ToTable("import_batches", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.FileUrl).HasColumnName("file_url").HasMaxLength(500);
            entity.Property(e => e.TotalRows).HasColumnName("total_rows").HasDefaultValue(0);
            entity.Property(e => e.SuccessCount).HasColumnName("success_count").HasDefaultValue(0);
            entity.Property(e => e.FailedCount).HasColumnName("failed_count").HasDefaultValue(0);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            entity.Property(e => e.ImportedBy).HasColumnName("imported_by");
            entity.Property(e => e.StartedAt).HasColumnName("started_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Class)
                .WithMany()
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Section)
                .WithMany()
                .HasForeignKey(e => e.SectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ImportBatchRow>(entity =>
        {
            entity.ToTable("import_batch_rows", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.RowNumber).HasColumnName("row_number");
            entity.Property(e => e.RawData).HasColumnName("raw_data").HasColumnType("jsonb");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.BatchId);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Batch)
                .WithMany(b => b.Rows)
                .HasForeignKey(e => e.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("departments", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.ToTable("designations", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(e => e.DesignationId).HasColumnName("designation_id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.JoiningDate).HasColumnName("joining_date").HasColumnType("date");
            entity.Property(e => e.Qualification).HasColumnName("qualification");
            entity.Property(e => e.ExperienceDetails).HasColumnName("experience_details");
            entity.Property(e => e.TotalExperience).HasColumnName("total_experience").HasMaxLength(100);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(20);
            entity.Property(e => e.Religion).HasColumnName("religion").HasMaxLength(100);
            entity.Property(e => e.BloodGroup).HasColumnName("blood_group").HasMaxLength(10);
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date");
            entity.Property(e => e.MobileNo).HasColumnName("mobile_no").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.PresentAddress).HasColumnName("present_address");
            entity.Property(e => e.PermanentAddress).HasColumnName("permanent_address");
            entity.Property(e => e.NidNumber).HasColumnName("nid_number").HasMaxLength(100);
            entity.Property(e => e.ProfilePictureUrl).HasColumnName("profile_picture_url").HasMaxLength(500);
            entity.Property(e => e.FacebookUrl).HasColumnName("facebook_url").HasMaxLength(500);
            entity.Property(e => e.TwitterUrl).HasColumnName("twitter_url").HasMaxLength(500);
            entity.Property(e => e.LinkedInUrl).HasColumnName("linkedin_url").HasMaxLength(500);
            entity.Property(e => e.SkipBankDetails).HasColumnName("skip_bank_details").HasDefaultValue(false);
            entity.Property(e => e.BankName).HasColumnName("bank_name").HasMaxLength(200);
            entity.Property(e => e.HolderName).HasColumnName("holder_name").HasMaxLength(200);
            entity.Property(e => e.BankBranch).HasColumnName("bank_branch").HasMaxLength(200);
            entity.Property(e => e.BankAddress).HasColumnName("bank_address");
            entity.Property(e => e.IfscCode).HasColumnName("ifsc_code").HasMaxLength(50);
            entity.Property(e => e.AccountNo).HasColumnName("account_no").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.StaffId).IsUnique();
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.DepartmentId);
            entity.HasIndex(e => e.DesignationId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Designation)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DesignationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EmployeeImportBatch>(entity =>
        {
            entity.ToTable("employee_import_batches", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.FileUrl).HasColumnName("file_url").HasMaxLength(500);
            entity.Property(e => e.TotalRows).HasColumnName("total_rows").HasDefaultValue(0);
            entity.Property(e => e.SuccessCount).HasColumnName("success_count").HasDefaultValue(0);
            entity.Property(e => e.FailedCount).HasColumnName("failed_count").HasDefaultValue(0);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            entity.Property(e => e.ImportedBy).HasColumnName("imported_by");
            entity.Property(e => e.StartedAt).HasColumnName("started_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<EmployeeImportBatchRow>(entity =>
        {
            entity.ToTable("employee_import_batch_rows", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.RowNumber).HasColumnName("row_number");
            entity.Property(e => e.RawData).HasColumnName("raw_data").HasColumnType("jsonb");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.BatchId);

            entity.HasOne(e => e.Batch)
                .WithMany(b => b.Rows)
                .HasForeignKey(e => e.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SalaryTemplate>(entity =>
        {
            entity.ToTable("salary_templates", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SalaryGrade).HasColumnName("salary_grade").HasMaxLength(100).IsRequired();
            entity.Property(e => e.BasicSalary).HasColumnName("basic_salary").HasPrecision(12, 2);
            entity.Property(e => e.OvertimeRatePerHour).HasColumnName("overtime_rate_per_hour").HasPrecision(10, 2);
            entity.Property(e => e.TotalAllowance).HasColumnName("total_allowance").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.TotalDeduction).HasColumnName("total_deduction").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.NetSalary).HasColumnName("net_salary").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<SalaryAllowance>(entity =>
        {
            entity.ToTable("salary_allowances", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasOne(e => e.Template).WithMany(t => t.Allowances).HasForeignKey(e => e.TemplateId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SalaryDeduction>(entity =>
        {
            entity.ToTable("salary_deductions", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasOne(e => e.Template).WithMany(t => t.Deductions).HasForeignKey(e => e.TemplateId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EmployeeSalaryAssignment>(entity =>
        {
            entity.ToTable("employee_salary_assignments", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.AssignedAt).HasColumnName("assigned_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.HasIndex(e => e.EmployeeId).IsUnique();
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Template).WithMany(t => t.Assignments).HasForeignKey(e => e.TemplateId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AssignedByUser).WithMany().HasForeignKey(e => e.AssignedBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SalaryPayment>(entity =>
        {
            entity.ToTable("salary_payments", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.PaymentMonth).HasColumnName("payment_month").HasMaxLength(7).IsRequired();
            entity.Property(e => e.BasicSalary).HasColumnName("basic_salary").HasPrecision(12, 2);
            entity.Property(e => e.TotalAllowance).HasColumnName("total_allowance").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.TotalDeduction).HasColumnName("total_deduction").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.NetSalary).HasColumnName("net_salary").HasPrecision(12, 2);
            entity.Property(e => e.OvertimeHours).HasColumnName("overtime_hours").HasPrecision(6, 2).HasDefaultValue(0m);
            entity.Property(e => e.OvertimeAmount).HasColumnName("overtime_amount").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.AdvanceDeduction).HasColumnName("advance_deduction").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.FinalAmount).HasColumnName("final_amount").HasPrecision(12, 2);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue(SalaryPaymentStatuses.Unpaid);
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
            entity.Property(e => e.PaymentNote).HasColumnName("payment_note");
            entity.Property(e => e.PaidBy).HasColumnName("paid_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.EmployeeId, e.PaymentMonth }).IsUnique();
            entity.HasIndex(e => e.PaymentMonth);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Template).WithMany().HasForeignKey(e => e.TemplateId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PaidByUser).WithMany().HasForeignKey(e => e.PaidBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AdvanceSalaryRequest>(entity =>
        {
            entity.ToTable("advance_salary_requests", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.DeductMonth).HasColumnName("deduct_month").HasMaxLength(7).IsRequired();
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(12, 2);
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue(HrRequestStatuses.Pending);
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.RejectReason).HasColumnName("reject_reason");
            entity.Property(e => e.AppliedOn).HasColumnName("applied_on").HasDefaultValueSql("NOW()");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.DeductMonth);
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Reviewer).WithMany().HasForeignKey(e => e.ReviewedBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LeaveCategory>(entity =>
        {
            entity.ToTable("leave_categories", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Days).HasColumnName("days");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.ToTable("leave_requests", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.LeaveCategoryId).HasColumnName("leave_category_id");
            entity.Property(e => e.DateOfStart).HasColumnName("date_of_start").HasColumnType("date");
            entity.Property(e => e.DateOfEnd).HasColumnName("date_of_end").HasColumnType("date");
            entity.Property(e => e.Days).HasColumnName("days");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.AttachmentUrl).HasColumnName("attachment_url").HasMaxLength(500);
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue(HrRequestStatuses.Pending);
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ApplyDate).HasColumnName("apply_date").HasDefaultValueSql("NOW()");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.LeaveCategoryId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.DateOfStart, e.DateOfEnd });
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.LeaveCategory).WithMany(c => c.LeaveRequests).HasForeignKey(e => e.LeaveCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Reviewer).WithMany().HasForeignKey(e => e.ReviewedBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Award>(entity =>
        {
            entity.ToTable("awards", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(e => e.AwardName).HasColumnName("award_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.GiftItem).HasColumnName("gift_item").HasMaxLength(200).IsRequired();
            entity.Property(e => e.CashPrice).HasColumnName("cash_price").HasPrecision(12, 2);
            entity.Property(e => e.AwardReason).HasColumnName("award_reason").HasMaxLength(500).IsRequired();
            entity.Property(e => e.GivenDate).HasColumnName("given_date").HasColumnType("date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.GivenDate);
            entity.ToTable(t => t.HasCheckConstraint(
                "chk_award_recipient",
                "(employee_id IS NOT NULL AND student_id IS NULL) OR (employee_id IS NULL AND student_id IS NOT NULL)"));
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClassTeacherAllocation>(entity =>
        {
            entity.ToTable("class_teacher_allocations", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.ClassId, e.SectionId }).IsUnique();
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("subjects", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Author).HasColumnName("author").HasMaxLength(200);
            entity.Property(e => e.SubjectType).HasColumnName("subject_type").HasMaxLength(50).IsRequired().HasDefaultValue(SubjectTypes.Theory);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<ClassSubjectAssignment>(entity =>
        {
            entity.ToTable("class_subject_assignments", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.ClassId, e.SectionId }).IsUnique();
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClassSubjectAssignmentItem>(entity =>
        {
            entity.ToTable("class_subject_assignment_items", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.HasIndex(e => new { e.AssignmentId, e.SubjectId }).IsUnique();
            entity.HasOne(e => e.Assignment).WithMany(a => a.Items).HasForeignKey(e => e.AssignmentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Subject).WithMany(s => s.AssignmentItems).HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.ToTable("class_schedules", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.Day).HasColumnName("day").HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.ClassId, e.SectionId, e.Day }).IsUnique();
            entity.HasIndex(e => new { e.ClassId, e.SectionId });
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClassSchedulePeriod>(entity =>
        {
            entity.ToTable("class_schedule_periods", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.IsBreak).HasColumnName("is_break").HasDefaultValue(false);
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.StartingTime).HasColumnName("starting_time").HasColumnType("time");
            entity.Property(e => e.EndingTime).HasColumnName("ending_time").HasColumnType("time");
            entity.Property(e => e.ClassRoom).HasColumnName("class_room").HasMaxLength(100);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasOne(e => e.Schedule).WithMany(s => s.Periods).HasForeignKey(e => e.ScheduleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Subject).WithMany().HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StudentPromotion>(entity =>
        {
            entity.ToTable("student_promotions", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.FromAcademicYear).HasColumnName("from_academic_year");
            entity.Property(e => e.FromClassId).HasColumnName("from_class_id");
            entity.Property(e => e.FromSectionId).HasColumnName("from_section_id");
            entity.Property(e => e.FromRoll).HasColumnName("from_roll").HasMaxLength(50);
            entity.Property(e => e.ToAcademicYear).HasColumnName("to_academic_year");
            entity.Property(e => e.ToClassId).HasColumnName("to_class_id");
            entity.Property(e => e.ToSectionId).HasColumnName("to_section_id");
            entity.Property(e => e.ToRoll).HasColumnName("to_roll").HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue(PromotionStatuses.Promoted);
            entity.Property(e => e.CurrentDueAmount).HasColumnName("current_due_amount").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.CarryForwardDue).HasColumnName("carry_forward_due").HasDefaultValue(true);
            entity.Property(e => e.PromotedBy).HasColumnName("promoted_by");
            entity.Property(e => e.PromotedAt).HasColumnName("promoted_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.StudentId);
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.FromClass).WithMany().HasForeignKey(e => e.FromClassId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.FromSection).WithMany().HasForeignKey(e => e.FromSectionId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ToClass).WithMany().HasForeignKey(e => e.ToClassId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ToSection).WithMany().HasForeignKey(e => e.ToSectionId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.PromotedByUser).WithMany().HasForeignKey(e => e.PromotedBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExamTerm>(entity =>
        {
            entity.ToTable("exam_terms", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<ExamHall>(entity =>
        {
            entity.ToTable("exam_halls", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.HallNo).HasColumnName("hall_no").HasMaxLength(50).IsRequired();
            entity.Property(e => e.NoOfSeats).HasColumnName("no_of_seats");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.HallNo).IsUnique();
        });

        modelBuilder.Entity<MarkDistribution>(entity =>
        {
            entity.ToTable("mark_distributions", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.ToTable("exams", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.ExamTermId).HasColumnName("exam_term_id");
            entity.Property(e => e.ExamType).HasColumnName("exam_type").HasMaxLength(100);
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(false);
            entity.Property(e => e.IsResultPublished).HasColumnName("is_result_published").HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasOne(e => e.ExamTerm).WithMany(t => t.Exams).HasForeignKey(e => e.ExamTermId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExamMarkDistribution>(entity =>
        {
            entity.ToTable("exam_mark_distributions", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.MarkDistributionId).HasColumnName("mark_distribution_id");
            entity.HasIndex(e => new { e.ExamId, e.MarkDistributionId }).IsUnique();
            entity.HasOne(e => e.Exam).WithMany(x => x.MarkDistributions).HasForeignKey(e => e.ExamId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.MarkDistribution).WithMany(x => x.ExamMarkDistributions).HasForeignKey(e => e.MarkDistributionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExamSchedule>(entity =>
        {
            entity.ToTable("exam_schedules", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.ExamId, e.ClassId, e.SectionId }).IsUnique();
            entity.HasOne(e => e.Exam).WithMany(x => x.Schedules).HasForeignKey(e => e.ExamId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExamScheduleSubject>(entity =>
        {
            entity.ToTable("exam_schedule_subjects", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.ExamDate).HasColumnName("exam_date").HasColumnType("date");
            entity.Property(e => e.StartingTime).HasColumnName("starting_time").HasColumnType("time");
            entity.Property(e => e.EndingTime).HasColumnName("ending_time").HasColumnType("time");
            entity.Property(e => e.HallId).HasColumnName("hall_id");
            entity.Property(e => e.WrittenFullMark).HasColumnName("written_full_mark");
            entity.Property(e => e.WrittenPassMark).HasColumnName("written_pass_mark");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasOne(e => e.Schedule).WithMany(s => s.Subjects).HasForeignKey(e => e.ScheduleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Subject).WithMany().HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Hall).WithMany().HasForeignKey(e => e.HallId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MarkEntry>(entity =>
        {
            entity.ToTable("mark_entries", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.IsAbsent).HasColumnName("is_absent").HasDefaultValue(false);
            entity.Property(e => e.WrittenMark).HasColumnName("written_mark").HasPrecision(6, 2);
            entity.Property(e => e.McqMark).HasColumnName("mcq_mark").HasPrecision(6, 2);
            entity.Property(e => e.TotalMark).HasColumnName("total_mark").HasPrecision(6, 2);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.ExamId, e.ClassId, e.SectionId, e.SubjectId, e.StudentId }).IsUnique();
            entity.HasIndex(e => e.ExamId).HasDatabaseName("idx_mark_entries_exam");
            entity.HasIndex(e => e.StudentId).HasDatabaseName("idx_mark_entries_student");
            entity.HasIndex(e => e.SubjectId).HasDatabaseName("idx_mark_entries_subject");
            entity.HasOne(e => e.Exam).WithMany(x => x.MarkEntries).HasForeignKey(e => e.ExamId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Subject).WithMany().HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
