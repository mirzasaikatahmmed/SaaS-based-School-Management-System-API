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
    public DbSet<StudentSubjectEnrollment> StudentSubjectEnrollments => Set<StudentSubjectEnrollment>();
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
    public DbSet<GradeRange> GradeRanges => Set<GradeRange>();
    public DbSet<ExamPosition> ExamPositions => Set<ExamPosition>();
    public DbSet<StudentAttendance> StudentAttendances => Set<StudentAttendance>();
    public DbSet<StudentSubjectAttendance> StudentSubjectAttendances => Set<StudentSubjectAttendance>();
    public DbSet<EmployeeAttendance> EmployeeAttendances => Set<EmployeeAttendance>();
    public DbSet<ExamAttendance> ExamAttendances => Set<ExamAttendance>();
    public DbSet<BookCategory> BookCategories => Set<BookCategory>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookIssue> BookIssues => Set<BookIssue>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<SchoolEvent> Events => Set<SchoolEvent>();
    public DbSet<OfflinePaymentType> OfflinePaymentTypes => Set<OfflinePaymentType>();
    public DbSet<OfflinePayment> OfflinePayments => Set<OfflinePayment>();
    public DbSet<FeesType> FeesTypes => Set<FeesType>();
    public DbSet<FeesGroup> FeesGroups => Set<FeesGroup>();
    public DbSet<FeesGroupItem> FeesGroupItems => Set<FeesGroupItem>();
    public DbSet<FineSetup> FineSetups => Set<FineSetup>();
    public DbSet<FeesAllocation> FeesAllocations => Set<FeesAllocation>();
    public DbSet<StudentFeeInvoice> StudentFeeInvoices => Set<StudentFeeInvoice>();
    public DbSet<FeesReminder> FeesReminders => Set<FeesReminder>();
    public DbSet<VoucherHead> VoucherHeads => Set<VoucherHead>();
    public DbSet<AccountingAccount> AccountingAccounts => Set<AccountingAccount>();
    public DbSet<AccountingDeposit> AccountingDeposits => Set<AccountingDeposit>();
    public DbSet<AccountingExpense> AccountingExpenses => Set<AccountingExpense>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageRecipient> MessageRecipients => Set<MessageRecipient>();
    public DbSet<SchoolSettings> SchoolSettings => Set<SchoolSettings>();
    public DbSet<BiometricDevice> BiometricDevices => Set<BiometricDevice>();
    public DbSet<BiometricUserMap> BiometricUserMaps => Set<BiometricUserMap>();
    public DbSet<BiometricPunchLog> BiometricPunchLogs => Set<BiometricPunchLog>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AcademicSession> AcademicSessions => Set<AcademicSession>();
    public DbSet<DatabaseBackup> DatabaseBackups => Set<DatabaseBackup>();
    public DbSet<EmailSettings> EmailSettings => Set<EmailSettings>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<SmsSettings> SmsSettings => Set<SmsSettings>();
    public DbSet<SmsTemplate> SmsTemplates => Set<SmsTemplate>();
    public DbSet<NotificationDispatchLog> NotificationDispatchLogs => Set<NotificationDispatchLog>();
    public DbSet<WebsiteCmsSettings> WebsiteCmsSettings => Set<WebsiteCmsSettings>();
    public DbSet<WebsiteMenuItem> WebsiteMenuItems => Set<WebsiteMenuItem>();
    public DbSet<WebsiteFooterLink> WebsiteFooterLinks => Set<WebsiteFooterLink>();
    public DbSet<WebsiteSliderItem> WebsiteSliderItems => Set<WebsiteSliderItem>();
    public DbSet<WebsiteImportantLink> WebsiteImportantLinks => Set<WebsiteImportantLink>();
    public DbSet<WebsiteSpeech> WebsiteSpeeches => Set<WebsiteSpeech>();
    public DbSet<WebsiteTenurePerson> WebsiteTenurePeople => Set<WebsiteTenurePerson>();
    public DbSet<WebsiteCommitteeMember> WebsiteCommitteeMembers => Set<WebsiteCommitteeMember>();
    public DbSet<WebsiteNotice> WebsiteNotices => Set<WebsiteNotice>();
    public DbSet<WebsiteGalleryCategory> WebsiteGalleryCategories => Set<WebsiteGalleryCategory>();
    public DbSet<WebsiteGalleryItem> WebsiteGalleryItems => Set<WebsiteGalleryItem>();
    public DbSet<WebsiteDocument> WebsiteDocuments => Set<WebsiteDocument>();
    public DbSet<WebsiteContentPage> WebsiteContentPages => Set<WebsiteContentPage>();
    public DbSet<WebsiteHandnote> WebsiteHandnotes => Set<WebsiteHandnote>();
    public DbSet<WebsiteOnlineClassVideo> WebsiteOnlineClassVideos => Set<WebsiteOnlineClassVideo>();
    public DbSet<WebsiteResultAnalyticsRow> WebsiteResultAnalyticsRows => Set<WebsiteResultAnalyticsRow>();
    public DbSet<WebsitePublishedResult> WebsitePublishedResults => Set<WebsitePublishedResult>();
    public DbSet<WebsiteVisitorDaily> WebsiteVisitorDailies => Set<WebsiteVisitorDaily>();
    public DbSet<WebsiteContactMessage> WebsiteContactMessages => Set<WebsiteContactMessage>();

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
            entity.Property(e => e.PasswordRevealEncrypted).HasColumnName("password_reveal_encrypted");
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
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Prefix).IsUnique();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.FeatureKey).HasColumnName("feature_key").HasMaxLength(150).IsRequired();
            entity.Property(e => e.CanView).HasColumnName("can_view").HasDefaultValue(false);
            entity.Property(e => e.CanAdd).HasColumnName("can_add").HasDefaultValue(false);
            entity.Property(e => e.CanEdit).HasColumnName("can_edit").HasDefaultValue(false);
            entity.Property(e => e.CanDelete).HasColumnName("can_delete").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => new { e.RoleId, e.FeatureKey }).IsUnique();
            entity.HasOne(e => e.Role).WithMany(r => r.Permissions).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
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
            entity.Property(e => e.CanBeAdditional).HasColumnName("can_be_additional").HasDefaultValue(false);
            entity.Property(e => e.IsContinuousAssessment).HasColumnName("is_continuous_assessment").HasDefaultValue(false);
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
            entity.Property(e => e.IsElective).HasColumnName("is_elective").HasDefaultValue(false);
            entity.Property(e => e.ElectiveGroup).HasColumnName("elective_group").HasMaxLength(50);
            entity.HasIndex(e => new { e.AssignmentId, e.SubjectId }).IsUnique();
            entity.HasOne(e => e.Assignment).WithMany(a => a.Items).HasForeignKey(e => e.AssignmentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Subject).WithMany(s => s.AssignmentItems).HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentSubjectEnrollment>(entity =>
        {
            entity.ToTable("student_subject_enrollments", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.AcademicYear).HasColumnName("academic_year");
            entity.Property(e => e.ElectiveGroup).HasColumnName("elective_group").HasMaxLength(50).IsRequired().HasDefaultValue("4th");
            entity.Property(e => e.AdditionalSubjectId).HasColumnName("additional_subject_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.StudentId, e.ElectiveGroup, e.AcademicYear }).IsUnique();
            entity.HasIndex(e => new { e.StudentId, e.SubjectId, e.AcademicYear }).IsUnique();
            entity.HasIndex(e => new { e.ClassId, e.SectionId, e.AcademicYear, e.ElectiveGroup });
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Subject).WithMany().HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AdditionalSubject).WithMany().HasForeignKey(e => e.AdditionalSubjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
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

        modelBuilder.Entity<GradeRange>(entity =>
        {
            entity.ToTable("grade_ranges", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.GradeName).HasColumnName("grade_name").HasMaxLength(10).IsRequired();
            entity.Property(e => e.GradePoint).HasColumnName("grade_point").HasPrecision(4, 2);
            entity.Property(e => e.MinPercentage).HasColumnName("min_percentage").HasPrecision(5, 2);
            entity.Property(e => e.MaxPercentage).HasColumnName("max_percentage").HasPrecision(5, 2);
            entity.Property(e => e.Remarks).HasColumnName("remarks").HasMaxLength(200);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.GradeName).IsUnique();
        });

        modelBuilder.Entity<ExamPosition>(entity =>
        {
            entity.ToTable("exam_positions", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.AcademicYear).HasColumnName("academic_year");
            entity.Property(e => e.TotalMarks).HasColumnName("total_marks").HasPrecision(8, 2).HasDefaultValue(0m);
            entity.Property(e => e.FullMarks).HasColumnName("full_marks").HasPrecision(8, 2).HasDefaultValue(0m);
            entity.Property(e => e.Percentage).HasColumnName("percentage").HasPrecision(5, 2).HasDefaultValue(0m);
            entity.Property(e => e.Gpa).HasColumnName("gpa").HasPrecision(4, 2).HasDefaultValue(0m);
            entity.Property(e => e.Result).HasColumnName("result").HasMaxLength(10).IsRequired().HasDefaultValue("FAIL");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.PrincipalComments).HasColumnName("principal_comments");
            entity.Property(e => e.TeacherComments).HasColumnName("teacher_comments");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.ExamId, e.ClassId, e.SectionId, e.StudentId }).IsUnique();
            entity.HasOne(e => e.Exam).WithMany().HasForeignKey(e => e.ExamId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentAttendance>(entity =>
        {
            entity.ToTable("student_attendance", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date").HasColumnType("date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("Present");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.StudentId, e.AttendanceDate }).IsUnique();
            entity.HasIndex(e => e.AttendanceDate).HasDatabaseName("idx_student_att_date");
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StudentSubjectAttendance>(entity =>
        {
            entity.ToTable("student_subject_attendance", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date").HasColumnType("date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("Present");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.StudentId, e.SubjectId, e.AttendanceDate }).IsUnique();
            entity.HasIndex(e => e.AttendanceDate).HasDatabaseName("idx_student_subject_att_date");
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Subject).WithMany().HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EmployeeAttendance>(entity =>
        {
            entity.ToTable("employee_attendance", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date").HasColumnType("date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.EmployeeId, e.AttendanceDate }).IsUnique();
            entity.HasIndex(e => e.AttendanceDate).HasDatabaseName("idx_employee_att_date");
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExamAttendance>(entity =>
        {
            entity.ToTable("exam_attendance", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("Present");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.ExamId, e.SubjectId, e.StudentId }).IsUnique();
            entity.HasOne(e => e.Exam).WithMany().HasForeignKey(e => e.ExamId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Subject).WithMany().HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.ToTable("book_categories", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("books", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
            entity.Property(e => e.IsbnNo).HasColumnName("isbn_no").HasMaxLength(100);
            entity.Property(e => e.Author).HasColumnName("author").HasMaxLength(300);
            entity.Property(e => e.Edition).HasColumnName("edition").HasMaxLength(100);
            entity.Property(e => e.Publisher).HasColumnName("publisher").HasMaxLength(300);
            entity.Property(e => e.PurchaseDate).HasColumnName("purchase_date").HasColumnType("date");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(10, 2);
            entity.Property(e => e.CoverImageUrl).HasColumnName("cover_image_url").HasMaxLength(500);
            entity.Property(e => e.TotalStock).HasColumnName("total_stock").HasDefaultValue(0);
            entity.Property(e => e.IssuedCopies).HasColumnName("issued_copies").HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasOne(e => e.Category).WithMany(c => c.Books).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<BookIssue>(entity =>
        {
            entity.ToTable("book_issues", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.UserName).HasColumnName("user_name").HasMaxLength(200);
            entity.Property(e => e.DateOfIssue).HasColumnName("date_of_issue").HasColumnType("date");
            entity.Property(e => e.DateOfExpiry).HasColumnName("date_of_expiry").HasColumnType("date");
            entity.Property(e => e.ReturnDate).HasColumnName("return_date").HasColumnType("date");
            entity.Property(e => e.Fine).HasColumnName("fine").HasPrecision(10, 2).HasDefaultValue(0m);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("Issued");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.BookId).HasDatabaseName("idx_book_issues_book");
            entity.HasIndex(e => e.Status).HasDatabaseName("idx_book_issues_status");
            entity.HasOne(e => e.Book).WithMany(b => b.Issues).HasForeignKey(e => e.BookId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.ToTable("event_types", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Icon).HasColumnName("icon").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<SchoolEvent>(entity =>
        {
            entity.ToTable("events", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
            entity.Property(e => e.EventTypeId).HasColumnName("event_type_id");
            entity.Property(e => e.IsHoliday).HasColumnName("is_holiday").HasDefaultValue(false);
            entity.Property(e => e.Audience).HasColumnName("audience").HasMaxLength(50).IsRequired().HasDefaultValue("Everybody");
            entity.Property(e => e.DateOfStart).HasColumnName("date_of_start").HasColumnType("date");
            entity.Property(e => e.DateOfEnd).HasColumnName("date_of_end").HasColumnType("date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
            entity.Property(e => e.ShowWebsite).HasColumnName("show_website").HasDefaultValue(false);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(false);
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.DateOfStart, e.DateOfEnd }).HasDatabaseName("idx_events_dates");
            entity.HasOne(e => e.EventType).WithMany(t => t.Events).HasForeignKey(e => e.EventTypeId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OfflinePaymentType>(entity =>
        {
            entity.ToTable("offline_payment_types", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Instructions).HasColumnName("instructions");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<OfflinePayment>(entity =>
        {
            entity.ToTable("offline_payments", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.TrxId).HasColumnName("trx_id").HasMaxLength(100).IsRequired();
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.PaymentTypeId).HasColumnName("payment_type_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date").HasColumnType("date");
            entity.Property(e => e.SubmitDate).HasColumnName("submit_date").HasDefaultValueSql("NOW()");
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(12, 2);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.TrxId).IsUnique();
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PaymentType).WithMany().HasForeignKey(e => e.PaymentTypeId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FeesType>(entity =>
        {
            entity.ToTable("fees_types", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.FeeCode).HasColumnName("fee_code").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.FeeCode).IsUnique();
        });

        modelBuilder.Entity<FeesGroup>(entity =>
        {
            entity.ToTable("fees_groups", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<FeesGroupItem>(entity =>
        {
            entity.ToTable("fees_group_items", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.FeesTypeId).HasColumnName("fees_type_id");
            entity.Property(e => e.DueDate).HasColumnName("due_date").HasColumnType("date");
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(12, 2);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasOne(e => e.Group).WithMany(g => g.Items).HasForeignKey(e => e.GroupId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.FeesType).WithMany(t => t.GroupItems).HasForeignKey(e => e.FeesTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FineSetup>(entity =>
        {
            entity.ToTable("fine_setups", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.FeesTypeId).HasColumnName("fees_type_id");
            entity.Property(e => e.FineType).HasColumnName("fine_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.FineValue).HasColumnName("fine_value").HasPrecision(10, 2);
            entity.Property(e => e.LateFeeFrequency).HasColumnName("late_fee_frequency").HasMaxLength(50);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.GroupId, e.FeesTypeId }).IsUnique();
            entity.HasOne(e => e.Group).WithMany().HasForeignKey(e => e.GroupId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.FeesType).WithMany().HasForeignKey(e => e.FeesTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FeesAllocation>(entity =>
        {
            entity.ToTable("fees_allocations", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.FeesGroupId).HasColumnName("fees_group_id");
            entity.Property(e => e.AcademicYear).HasColumnName("academic_year");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.ClassId, e.SectionId, e.FeesGroupId, e.AcademicYear }).IsUnique();
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.FeesGroup).WithMany(g => g.Allocations).HasForeignKey(e => e.FeesGroupId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentFeeInvoice>(entity =>
        {
            entity.ToTable("student_fee_invoices", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.FeesAllocationId).HasColumnName("fees_allocation_id");
            entity.Property(e => e.FeesGroupId).HasColumnName("fees_group_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.PaidAmount).HasColumnName("paid_amount").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.FineAmount).HasColumnName("fine_amount").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.DueAmount).HasColumnName("due_amount").HasPrecision(12, 2).HasDefaultValue(0m);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("Unpaid");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.StudentId).HasDatabaseName("idx_invoices_student");
            entity.HasIndex(e => e.Status).HasDatabaseName("idx_invoices_status");
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.FeesAllocation).WithMany(a => a.Invoices).HasForeignKey(e => e.FeesAllocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.FeesGroup).WithMany().HasForeignKey(e => e.FeesGroupId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FeesReminder>(entity =>
        {
            entity.ToTable("fees_reminders", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Frequency).HasColumnName("frequency").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Days).HasColumnName("days");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.DltTemplateId).HasColumnName("dlt_template_id").HasMaxLength(200);
            entity.Property(e => e.NotifyStudent).HasColumnName("notify_student").HasDefaultValue(false);
            entity.Property(e => e.NotifyGuardian).HasColumnName("notify_guardian").HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<VoucherHead>(entity =>
        {
            entity.ToTable("voucher_heads", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(20).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<AccountingAccount>(entity =>
        {
            entity.ToTable("accounting_accounts", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AccountName).HasColumnName("account_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.AccountNumber).HasColumnName("account_number").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.OpeningBalance).HasColumnName("opening_balance").HasPrecision(14, 2).HasDefaultValue(0m);
            entity.Property(e => e.CurrentBalance).HasColumnName("current_balance").HasPrecision(14, 2).HasDefaultValue(0m);
            entity.Property(e => e.Date).HasColumnName("date").HasColumnType("date");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<AccountingDeposit>(entity =>
        {
            entity.ToTable("accounting_deposits", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.VoucherHeadId).HasColumnName("voucher_head_id");
            entity.Property(e => e.RefNo).HasColumnName("ref_no").HasMaxLength(200);
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(14, 2);
            entity.Property(e => e.DepositDate).HasColumnName("deposit_date").HasColumnType("date");
            entity.Property(e => e.PayVia).HasColumnName("pay_via").HasMaxLength(50);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.AttachmentUrl).HasColumnName("attachment_url").HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.AccountId).HasDatabaseName("idx_deposits_account");
            entity.HasOne(e => e.Account).WithMany(a => a.Deposits).HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.VoucherHead).WithMany().HasForeignKey(e => e.VoucherHeadId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AccountingExpense>(entity =>
        {
            entity.ToTable("accounting_expenses", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.VoucherHeadId).HasColumnName("voucher_head_id");
            entity.Property(e => e.RefNo).HasColumnName("ref_no").HasMaxLength(200);
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(14, 2);
            entity.Property(e => e.ExpenseDate).HasColumnName("expense_date").HasColumnType("date");
            entity.Property(e => e.PayVia).HasColumnName("pay_via").HasMaxLength(50);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.AttachmentUrl).HasColumnName("attachment_url").HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.AccountId).HasDatabaseName("idx_expenses_account");
            entity.HasOne(e => e.Account).WithMany(a => a.Expenses).HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.VoucherHead).WithMany().HasForeignKey(e => e.VoucherHeadId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SenderName).HasColumnName("sender_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.ParentMessageId).HasColumnName("parent_message_id");
            entity.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Body).HasColumnName("body").IsRequired();
            entity.Property(e => e.AttachmentUrl).HasColumnName("attachment_url").HasMaxLength(500);
            entity.Property(e => e.AttachmentName).HasColumnName("attachment_name").HasMaxLength(255);
            entity.Property(e => e.IsDeletedBySender).HasColumnName("is_deleted_by_sender").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.SenderId).HasDatabaseName("idx_messages_sender");
            entity.HasOne(e => e.Sender).WithMany().HasForeignKey(e => e.SenderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ParentMessage).WithMany().HasForeignKey(e => e.ParentMessageId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MessageRecipient>(entity =>
        {
            entity.ToTable("message_recipients", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.RecipientId).HasColumnName("recipient_id");
            entity.Property(e => e.RecipientName).HasColumnName("recipient_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.IsImportant).HasColumnName("is_important").HasDefaultValue(false);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.RecipientId).HasDatabaseName("idx_message_recipients_recipient");
            entity.HasIndex(e => e.MessageId).HasDatabaseName("idx_message_recipients_message");
            entity.HasOne(e => e.Message).WithMany(m => m.Recipients).HasForeignKey(e => e.MessageId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Recipient).WithMany().HasForeignKey(e => e.RecipientId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SchoolSettings>(entity =>
        {
            entity.ToTable("school_settings", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SchoolName).HasColumnName("school_name").HasMaxLength(200);
            entity.Property(e => e.SchoolCode).HasColumnName("school_code").HasMaxLength(50);
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.Website).HasColumnName("website").HasMaxLength(255);
            entity.Property(e => e.Timezone).HasColumnName("timezone").HasMaxLength(100).HasDefaultValue("Asia/Dhaka");
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(10).HasDefaultValue("BDT");
            entity.Property(e => e.CurrencySymbol).HasColumnName("currency_symbol").HasMaxLength(10).HasDefaultValue("৳");
            entity.Property(e => e.DateFormat).HasColumnName("date_format").HasMaxLength(20).HasDefaultValue("DD/MM/YYYY");
            entity.Property(e => e.Language).HasColumnName("language").HasMaxLength(20).HasDefaultValue("en");
            entity.Property(e => e.AllowStudentLogin).HasColumnName("allow_student_login").HasDefaultValue(true);
            entity.Property(e => e.AllowGuardianLogin).HasColumnName("allow_guardian_login").HasDefaultValue(true);
            entity.Property(e => e.ShowFeesInStudentPanel).HasColumnName("show_fees_in_student_panel").HasDefaultValue(true);
            entity.Property(e => e.ShowAttendanceInStudentPanel).HasColumnName("show_attendance_in_student_panel").HasDefaultValue(true);
            entity.Property(e => e.ShowResultInStudentPanel).HasColumnName("show_result_in_student_panel").HasDefaultValue(true);
            entity.Property(e => e.StudentPanelNoticeMessage).HasColumnName("student_panel_notice_message");
            entity.Property(e => e.SystemLogoUrl).HasColumnName("system_logo_url").HasMaxLength(500);
            entity.Property(e => e.TextLogoUrl).HasColumnName("text_logo_url").HasMaxLength(500);
            entity.Property(e => e.PrintingLogoUrl).HasColumnName("printing_logo_url").HasMaxLength(500);
            entity.Property(e => e.ReportCardLogoUrl).HasColumnName("report_card_logo_url").HasMaxLength(500);
            entity.Property(e => e.PaymentGateways).HasColumnName("payment_gateways").HasColumnType("jsonb").HasDefaultValue("{}");
            entity.Property(e => e.ActiveGateways).HasColumnName("active_gateways").HasColumnType("jsonb").HasDefaultValue("[]");
            entity.Property(e => e.AttendanceType).HasColumnName("attendance_type").HasMaxLength(20).HasDefaultValue("DayWise");
            entity.Property(e => e.WeekendDays).HasColumnName("weekend_days").HasMaxLength(20).HasDefaultValue("5,6");
            entity.Property(e => e.DefaultDepositAccountId).HasColumnName("default_deposit_account_id");
            entity.Property(e => e.DefaultExpenseAccountId).HasColumnName("default_expense_account_id");
            entity.Property(e => e.AccountingLinksEnabled).HasColumnName("accounting_links_enabled").HasDefaultValue(false);
            entity.Property(e => e.CronSecretKey).HasColumnName("cron_secret_key").HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<BiometricDevice>(entity =>
        {
            entity.ToTable("biometric_devices", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SerialNumber).HasColumnName("serial_number").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(200);
            entity.Property(e => e.DeviceModel).HasColumnName("device_model").HasMaxLength(50).HasDefaultValue("K40-H");
            entity.Property(e => e.ExamGraceMinutesBefore).HasColumnName("exam_grace_minutes_before").HasDefaultValue(30);
            entity.Property(e => e.ExamGraceMinutesAfter).HasColumnName("exam_grace_minutes_after").HasDefaultValue(30);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.LastSeenAt).HasColumnName("last_seen_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.SerialNumber).IsUnique();
        });

        modelBuilder.Entity<BiometricUserMap>(entity =>
        {
            entity.ToTable("biometric_user_maps", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DevicePin).HasColumnName("device_pin").HasMaxLength(50).IsRequired();
            entity.Property(e => e.PersonType).HasColumnName("person_type").HasMaxLength(20).IsRequired().HasDefaultValue("Student");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.DevicePin).IsUnique();
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BiometricPunchLog>(entity =>
        {
            entity.ToTable("biometric_punch_logs", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.DeviceSn).HasColumnName("device_sn").HasMaxLength(100).IsRequired();
            entity.Property(e => e.DevicePin).HasColumnName("device_pin").HasMaxLength(50).IsRequired();
            entity.Property(e => e.PunchTime).HasColumnName("punch_time");
            entity.Property(e => e.PunchKind).HasColumnName("punch_kind").HasMaxLength(20).IsRequired().HasDefaultValue("Unmapped");
            entity.Property(e => e.StatusApplied).HasColumnName("status_applied").HasMaxLength(20).HasDefaultValue("Present");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.RawLine).HasColumnName("raw_line").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.PunchTime).HasDatabaseName("idx_biometric_punch_logs_time");
            entity.HasIndex(e => e.DevicePin).HasDatabaseName("idx_biometric_punch_logs_pin");
            entity.HasOne(e => e.Device).WithMany(d => d.PunchLogs).HasForeignKey(e => e.DeviceId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Student).WithMany().HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AcademicSession>(entity =>
        {
            entity.ToTable("academic_sessions", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
            entity.Property(e => e.IsSelected).HasColumnName("is_selected").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<DatabaseBackup>(entity =>
        {
            entity.ToTable("database_backups", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.ObjectKey).HasColumnName("object_key").HasMaxLength(500).IsRequired();
            entity.Property(e => e.SizeBytes).HasColumnName("size_bytes").HasDefaultValue(0L);
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<EmailSettings>(entity =>
        {
            entity.ToTable("email_settings", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled").HasDefaultValue(false);
            entity.Property(e => e.SystemEmail).HasColumnName("system_email").HasMaxLength(255);
            entity.Property(e => e.Protocol).HasColumnName("protocol").HasMaxLength(20).HasDefaultValue("SMTP");
            entity.Property(e => e.SmtpHost).HasColumnName("smtp_host").HasMaxLength(255);
            entity.Property(e => e.SmtpPort).HasColumnName("smtp_port").HasDefaultValue(587);
            entity.Property(e => e.SmtpUsername).HasColumnName("smtp_username").HasMaxLength(255);
            entity.Property(e => e.SmtpPassword).HasColumnName("smtp_password").HasMaxLength(1000);
            entity.Property(e => e.SmtpSecure).HasColumnName("smtp_secure").HasMaxLength(20).HasDefaultValue("TLS");
            entity.Property(e => e.SmtpAuth).HasColumnName("smtp_auth").HasDefaultValue(true);
            entity.Property(e => e.FromName).HasColumnName("from_name").HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.ToTable("email_templates", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EventKey).HasColumnName("event_key").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(500).IsRequired();
            entity.Property(e => e.BodyHtml).HasColumnName("body_html").IsRequired();
            entity.Property(e => e.NotifyEnabled).HasColumnName("notify_enabled").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.EventKey).IsUnique();
        });

        modelBuilder.Entity<SmsSettings>(entity =>
        {
            entity.ToTable("sms_settings", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled").HasDefaultValue(false);
            entity.Property(e => e.ActivatedGateway).HasColumnName("activated_gateway").HasMaxLength(50).HasDefaultValue("bulksmsbd");
            entity.Property(e => e.CredentialsJson).HasColumnName("credentials_json").HasColumnType("jsonb").HasDefaultValue("{}");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<SmsTemplate>(entity =>
        {
            entity.ToTable("sms_templates", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EventKey).HasColumnName("event_key").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Body).HasColumnName("body").IsRequired();
            entity.Property(e => e.NotifyStudent).HasColumnName("notify_student").HasDefaultValue(false);
            entity.Property(e => e.NotifyParent).HasColumnName("notify_parent").HasDefaultValue(true);
            entity.Property(e => e.DltTemplateId).HasColumnName("dlt_template_id").HasMaxLength(100);
            entity.Property(e => e.NotifyEnabled).HasColumnName("notify_enabled").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.EventKey).IsUnique();
        });

        modelBuilder.Entity<NotificationDispatchLog>(entity =>
        {
            entity.ToTable("notification_dispatch_log", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.JobName).HasColumnName("job_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityKey).HasColumnName("entity_key").HasMaxLength(200).IsRequired();
            entity.Property(e => e.RunDate).HasColumnName("run_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.JobName, e.EntityKey, e.RunDate }).IsUnique();
        });

        modelBuilder.Entity<WebsiteCmsSettings>(entity =>
        {
            entity.ToTable("website_cms_settings", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SchoolNameBn).HasColumnName("school_name_bn").HasMaxLength(300);
            entity.Property(e => e.FacebookUrl).HasColumnName("facebook_url").HasMaxLength(500);
            entity.Property(e => e.YoutubeUrl).HasColumnName("youtube_url").HasMaxLength(500);
            entity.Property(e => e.FacebookPageUrl).HasColumnName("facebook_page_url").HasMaxLength(500);
            entity.Property(e => e.PortalUrl).HasColumnName("portal_url").HasMaxLength(300).HasDefaultValue("/portal");
            entity.Property(e => e.CopyrightText).HasColumnName("copyright_text").HasMaxLength(500);
            entity.Property(e => e.OnlineAdmissionEnabled).HasColumnName("online_admission_enabled").HasDefaultValue(true);
            entity.Property(e => e.Eiin).HasColumnName("eiin").HasMaxLength(50);
            entity.Property(e => e.EstablishedYear).HasColumnName("established_year");
            entity.Property(e => e.SchoolType).HasColumnName("school_type").HasMaxLength(100);
            entity.Property(e => e.ClassesOffered).HasColumnName("classes_offered").HasMaxLength(200);
            entity.Property(e => e.TotalStudentsLabel).HasColumnName("total_students_label").HasMaxLength(50);
            entity.Property(e => e.HistoryImageUrl).HasColumnName("history_image_url").HasMaxLength(500);
            entity.Property(e => e.HistoryTitle).HasColumnName("history_title").HasMaxLength(200);
            entity.Property(e => e.HistoryTitleBn).HasColumnName("history_title_bn").HasMaxLength(200);
            entity.Property(e => e.HistorySectionsJson).HasColumnName("history_sections_json").HasColumnType("jsonb").HasDefaultValue("[]");
            entity.Property(e => e.FoundingCommitteeJson).HasColumnName("founding_committee_json").HasColumnType("jsonb").HasDefaultValue("[]");
            entity.Property(e => e.ContactPageTitle).HasColumnName("contact_page_title").HasMaxLength(200);
            entity.Property(e => e.ContactBoxTitle).HasColumnName("contact_box_title").HasMaxLength(300);
            entity.Property(e => e.ContactBoxDescription).HasColumnName("contact_box_description").HasMaxLength(1000);
            entity.Property(e => e.ContactMapIframeHtml).HasColumnName("contact_map_iframe_html");
            entity.Property(e => e.ContactSubmitButtonText).HasColumnName("contact_submit_button_text").HasMaxLength(100).HasDefaultValue("Send");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<WebsiteMenuItem>(entity =>
        {
            entity.ToTable("website_menu_items", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            entity.Property(e => e.TitleBn).HasColumnName("title_bn").HasMaxLength(200);
            entity.Property(e => e.Path).HasColumnName("path").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.OpenInNewTab).HasColumnName("open_in_new_tab").HasDefaultValue(false);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
            entity.HasOne(e => e.Parent).WithMany(e => e.Children).HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WebsiteFooterLink>(entity =>
        {
            entity.ToTable("website_footer_links", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ColumnKey).HasColumnName("column_key").HasMaxLength(50).IsRequired();
            entity.Property(e => e.ColumnTitle).HasColumnName("column_title").HasMaxLength(200).IsRequired();
            entity.Property(e => e.ColumnTitleBn).HasColumnName("column_title_bn").HasMaxLength(200);
            entity.Property(e => e.Label).HasColumnName("label").HasMaxLength(200).IsRequired();
            entity.Property(e => e.LabelBn).HasColumnName("label_bn").HasMaxLength(200);
            entity.Property(e => e.Path).HasColumnName("path").HasMaxLength(500).IsRequired();
            entity.Property(e => e.IsExternal).HasColumnName("is_external").HasDefaultValue(false);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
        });

        modelBuilder.Entity<WebsiteSliderItem>(entity =>
        {
            entity.ToTable("website_slider_items", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Caption).HasColumnName("caption").HasMaxLength(500);
            entity.Property(e => e.ButtonText).HasColumnName("button_text").HasMaxLength(100);
            entity.Property(e => e.ButtonUrl).HasColumnName("button_url").HasMaxLength(500);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
        });

        modelBuilder.Entity<WebsiteImportantLink>(entity =>
        {
            entity.ToTable("website_important_links", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Label).HasColumnName("label").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Url).HasColumnName("url").HasMaxLength(500).IsRequired();
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
        });

        modelBuilder.Entity<WebsiteSpeech>(entity =>
        {
            entity.ToTable("website_speeches", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            entity.Property(e => e.TitleBn).HasColumnName("title_bn").HasMaxLength(200);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(200);
            entity.Property(e => e.Designation).HasColumnName("designation").HasMaxLength(200).IsRequired();
            entity.Property(e => e.DesignationBn).HasColumnName("designation_bn").HasMaxLength(200);
            entity.Property(e => e.PhotoUrl).HasColumnName("photo_url").HasMaxLength(500);
            entity.Property(e => e.MessageHtml).HasColumnName("message_html").IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(100);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.FacebookUrl).HasColumnName("facebook_url").HasMaxLength(500);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.Role);
        });

        modelBuilder.Entity<WebsiteTenurePerson>(entity =>
        {
            entity.ToTable("website_tenure_people", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Kind).HasColumnName("kind").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
            entity.Property(e => e.Designation).HasColumnName("designation").HasMaxLength(300);
            entity.Property(e => e.JoinedOn).HasColumnName("joined_on");
            entity.Property(e => e.LeftOn).HasColumnName("left_on");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
            entity.HasIndex(e => e.Kind);
        });

        modelBuilder.Entity<WebsiteCommitteeMember>(entity =>
        {
            entity.ToTable("website_committee_members", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
            entity.Property(e => e.CategoryBn).HasColumnName("category_bn").HasMaxLength(200);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Designation).HasColumnName("designation").HasMaxLength(200).IsRequired();
            entity.Property(e => e.PhotoUrl).HasColumnName("photo_url").HasMaxLength(500);
            entity.Property(e => e.MobileNo).HasColumnName("mobile_no").HasMaxLength(100);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
        });

        modelBuilder.Entity<WebsiteNotice>(entity =>
        {
            entity.ToTable("website_notices", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.PublishedOn).HasColumnName("published_on");
            entity.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(500).IsRequired();
            entity.Property(e => e.BodyHtml).HasColumnName("body_html");
            entity.Property(e => e.FileUrl).HasColumnName("file_url").HasMaxLength(500);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<WebsiteGalleryCategory>(entity =>
        {
            entity.ToTable("website_gallery_categories", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        });

        modelBuilder.Entity<WebsiteGalleryItem>(entity =>
        {
            entity.ToTable("website_gallery_items", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ThumbUrl).HasColumnName("thumb_url").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ExtraImagesJson).HasColumnName("extra_images_json").HasColumnType("jsonb").HasDefaultValue("[]");
            entity.Property(e => e.EventDate).HasColumnName("event_date");
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.HasOne(e => e.Category).WithMany(c => c.Items).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WebsiteDocument>(entity =>
        {
            entity.ToTable("website_documents", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(400).IsRequired();
            entity.Property(e => e.TitleBn).HasColumnName("title_bn").HasMaxLength(400);
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50).IsRequired().HasDefaultValue("other");
            entity.Property(e => e.FileUrl).HasColumnName("file_url").HasMaxLength(500).IsRequired();
            entity.Property(e => e.PublishedOn).HasColumnName("published_on");
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        });

        modelBuilder.Entity<WebsiteContentPage>(entity =>
        {
            entity.ToTable("website_content_pages", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
            entity.Property(e => e.TitleBn).HasColumnName("title_bn").HasMaxLength(300);
            entity.Property(e => e.BodyHtml).HasColumnName("body_html");
            entity.Property(e => e.FileUrl).HasColumnName("file_url").HasMaxLength(500);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<WebsiteHandnote>(entity =>
        {
            entity.ToTable("website_handnotes", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.PublishedOn).HasColumnName("published_on");
            entity.Property(e => e.ClassName).HasColumnName("class_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
            entity.Property(e => e.TeacherName).HasColumnName("teacher_name").HasMaxLength(200);
            entity.Property(e => e.FileUrl).HasColumnName("file_url").HasMaxLength(500).IsRequired();
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        });

        modelBuilder.Entity<WebsiteOnlineClassVideo>(entity =>
        {
            entity.ToTable("website_online_class_videos", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ClassName).HasColumnName("class_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(400).IsRequired();
            entity.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(200);
            entity.Property(e => e.TeacherName).HasColumnName("teacher_name").HasMaxLength(200);
            entity.Property(e => e.YoutubeUrl).HasColumnName("youtube_url").HasMaxLength(500).IsRequired();
            entity.Property(e => e.YoutubeVideoId).HasColumnName("youtube_video_id").HasMaxLength(50);
            entity.Property(e => e.ClassDate).HasColumnName("class_date");
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.HasIndex(e => e.ClassName);
        });

        modelBuilder.Entity<WebsiteResultAnalyticsRow>(entity =>
        {
            entity.ToTable("website_result_analytics", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ExamType).HasColumnName("exam_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Appeared).HasColumnName("appeared").HasDefaultValue(0);
            entity.Property(e => e.Passed).HasColumnName("passed").HasDefaultValue(0);
            entity.Property(e => e.NotPassed).HasColumnName("not_passed").HasDefaultValue(0);
            entity.Property(e => e.PassPercent).HasColumnName("pass_percent").HasPrecision(6, 2);
            entity.Property(e => e.Gpa5).HasColumnName("gpa5").HasDefaultValue(0);
            entity.Property(e => e.Gpa5Percent).HasColumnName("gpa5_percent").HasPrecision(6, 2);
            entity.Property(e => e.Gpa4x).HasColumnName("gpa4x").HasDefaultValue(0);
            entity.Property(e => e.Gpa3x).HasColumnName("gpa3x").HasDefaultValue(0);
            entity.Property(e => e.Gpa2x).HasColumnName("gpa2x").HasDefaultValue(0);
            entity.Property(e => e.Gpa1x).HasColumnName("gpa1x").HasDefaultValue(0);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
            entity.HasIndex(e => new { e.ExamType, e.Year }).IsUnique();
        });

        modelBuilder.Entity<WebsitePublishedResult>(entity =>
        {
            entity.ToTable("website_published_results", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(400).IsRequired();
            entity.Property(e => e.TitleBn).HasColumnName("title_bn").HasMaxLength(400);
            entity.Property(e => e.ExamType).HasColumnName("exam_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.DetailUrl).HasColumnName("detail_url").HasMaxLength(500);
            entity.Property(e => e.FileUrl).HasColumnName("file_url").HasMaxLength(500);
            entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        });

        modelBuilder.Entity<WebsiteVisitorDaily>(entity =>
        {
            entity.ToTable("website_visitor_daily", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.VisitDate).HasColumnName("visit_date");
            entity.Property(e => e.Views).HasColumnName("views").HasDefaultValue(0);
            entity.HasIndex(e => e.VisitDate).IsUnique();
        });

        modelBuilder.Entity<WebsiteContactMessage>(entity =>
        {
            entity.ToTable("website_contact_messages", schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(100);
            entity.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(300);
            entity.Property(e => e.Message).HasColumnName("message").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
        });
    }
}
