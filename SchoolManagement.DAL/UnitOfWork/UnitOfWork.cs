using Microsoft.EntityFrameworkCore.Storage;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Repositories.Implementations;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;

namespace SchoolManagement.DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly MasterDbContext _masterContext;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantDbContextFactory _tenantDbContextFactory;
    private TenantDbContext? _tenantContextDb;
    private ITenantRepository? _tenantRepository;
    private ISchoolRepository? _schoolRepository;
    private IUserRepository? _userRepository;
    private IStudentRepository? _studentRepository;
    private IGuardianRepository? _guardianRepository;
    private IAdmissionLookupRepository? _admissionLookupRepository;
    private IOnlineAdmissionRepository? _onlineAdmissionRepository;
    private IImportRepository? _importRepository;
    private IStudentCategoryRepository? _studentCategoryRepository;
    private IDeactivateReasonRepository? _deactivateReasonRepository;
    private IDepartmentRepository? _departmentRepository;
    private IDesignationRepository? _designationRepository;
    private IEmployeeRepository? _employeeRepository;
    private ISalaryTemplateRepository? _salaryTemplateRepository;
    private ISalaryAssignmentRepository? _salaryAssignmentRepository;
    private ISalaryPaymentRepository? _salaryPaymentRepository;
    private IAdvanceSalaryRepository? _advanceSalaryRepository;
    private ILeaveCategoryRepository? _leaveCategoryRepository;
    private ILeaveRequestRepository? _leaveRequestRepository;
    private IAwardRepository? _awardRepository;
    private IClassControlRepository? _classControlRepository;
    private ISectionControlRepository? _sectionControlRepository;
    private IClassTeacherRepository? _classTeacherRepository;
    private ISubjectRepository? _subjectRepository;
    private IClassSubjectAssignmentRepository? _classSubjectAssignmentRepository;
    private IClassScheduleRepository? _classScheduleRepository;
    private IStudentPromotionRepository? _studentPromotionRepository;
    private IExamTermRepository? _examTermRepository;
    private IExamHallRepository? _examHallRepository;
    private IMarkDistributionRepository? _markDistributionRepository;
    private IExamRepository? _examRepository;
    private IExamScheduleRepository? _examScheduleRepository;
    private IMarkEntryRepository? _markEntryRepository;
    private IGradeRangeRepository? _gradeRangeRepository;
    private IExamPositionRepository? _examPositionRepository;
    private IStudentAttendanceRepository? _studentAttendanceRepository;
    private IEmployeeAttendanceRepository? _employeeAttendanceRepository;
    private IExamAttendanceRepository? _examAttendanceRepository;
    private IBookCategoryRepository? _bookCategoryRepository;
    private IBookRepository? _bookRepository;
    private IBookIssueRepository? _bookIssueRepository;
    private IEventTypeRepository? _eventTypeRepository;
    private IEventRepository? _eventRepository;
    private IFeesTypeRepository? _feesTypeRepository;
    private IFeesGroupRepository? _feesGroupRepository;
    private IFeesGroupItemRepository? _feesGroupItemRepository;
    private IFeesAllocationRepository? _feesAllocationRepository;
    private IFineSetupRepository? _fineSetupRepository;
    private IFeesReminderRepository? _feesReminderRepository;
    private IStudentFeeInvoiceRepository? _studentFeeInvoiceRepository;
    private IOfflinePaymentRepository? _offlinePaymentRepository;
    private IOfflinePaymentTypeRepository? _offlinePaymentTypeRepository;
    private IAccountingAccountRepository? _accountingAccountRepository;
    private IAccountingDepositRepository? _accountingDepositRepository;
    private IAccountingExpenseRepository? _accountingExpenseRepository;
    private IVoucherHeadRepository? _voucherHeadRepository;
    private IMessageRepository? _messageRepository;
    private ISchoolSettingsRepository? _schoolSettingsRepository;
    private IGlobalSettingsRepository? _globalSettingsRepository;
    private IBiometricDeviceRepository? _biometricDeviceRepository;
    private IBiometricUserMapRepository? _biometricUserMapRepository;
    private IBiometricPunchLogRepository? _biometricPunchLogRepository;
    private IBiometricDeviceRegistryRepository? _biometricDeviceRegistryRepository;
    private IRoleRepository? _roleRepository;
    private IAcademicSessionRepository? _academicSessionRepository;
    private IDatabaseBackupRepository? _databaseBackupRepository;
    private ILoginLogRepository? _loginLogRepository;
    private IEmailSettingsRepository? _emailSettingsRepository;
    private ISmsSettingsRepository? _smsSettingsRepository;
    private ICronSecretRegistryRepository? _cronSecretRegistryRepository;
    private IDbContextTransaction? _tenantTransaction;
    private bool _disposed;

    public UnitOfWork(
        MasterDbContext masterContext,
        ITenantContext tenantContext,
        ITenantDbContextFactory tenantDbContextFactory)
    {
        _masterContext = masterContext;
        _tenantContext = tenantContext;
        _tenantDbContextFactory = tenantDbContextFactory;
    }

    public ITenantRepository Tenants =>
        _tenantRepository ??= new TenantRepository(_masterContext);

    public ISchoolRepository Schools =>
        _schoolRepository ??= new SchoolRepository(_masterContext);

    public IUserRepository Users
    {
        get
        {
            EnsureTenantDb();
            return _userRepository ??= new UserRepository(_tenantContextDb!);
        }
    }

    public IStudentRepository Students
    {
        get
        {
            EnsureTenantDb();
            return _studentRepository ??= new StudentRepository(_tenantContextDb!);
        }
    }

    public IGuardianRepository Guardians
    {
        get
        {
            EnsureTenantDb();
            return _guardianRepository ??= new GuardianRepository(_tenantContextDb!);
        }
    }

    public IAdmissionLookupRepository AdmissionLookups
    {
        get
        {
            EnsureTenantDb();
            return _admissionLookupRepository ??= new AdmissionLookupRepository(_tenantContextDb!);
        }
    }

    public IOnlineAdmissionRepository OnlineAdmissions
    {
        get
        {
            EnsureTenantDb();
            return _onlineAdmissionRepository ??= new OnlineAdmissionRepository(_tenantContextDb!);
        }
    }

    public IImportRepository Imports
    {
        get
        {
            EnsureTenantDb();
            return _importRepository ??= new ImportRepository(_tenantContextDb!);
        }
    }

    public IStudentCategoryRepository StudentCategories
    {
        get
        {
            EnsureTenantDb();
            return _studentCategoryRepository ??= new StudentCategoryRepository(_tenantContextDb!);
        }
    }

    public IDeactivateReasonRepository DeactivateReasons
    {
        get
        {
            EnsureTenantDb();
            return _deactivateReasonRepository ??= new DeactivateReasonRepository(_tenantContextDb!);
        }
    }

    public IDepartmentRepository Departments
    {
        get
        {
            EnsureTenantDb();
            return _departmentRepository ??= new DepartmentRepository(_tenantContextDb!);
        }
    }

    public IDesignationRepository Designations
    {
        get
        {
            EnsureTenantDb();
            return _designationRepository ??= new DesignationRepository(_tenantContextDb!);
        }
    }

    public IEmployeeRepository Employees
    {
        get
        {
            EnsureTenantDb();
            return _employeeRepository ??= new EmployeeRepository(_tenantContextDb!);
        }
    }

    public ISalaryTemplateRepository SalaryTemplates
    {
        get
        {
            EnsureTenantDb();
            return _salaryTemplateRepository ??= new SalaryTemplateRepository(_tenantContextDb!);
        }
    }

    public ISalaryAssignmentRepository SalaryAssignments
    {
        get
        {
            EnsureTenantDb();
            return _salaryAssignmentRepository ??= new SalaryAssignmentRepository(_tenantContextDb!);
        }
    }

    public ISalaryPaymentRepository SalaryPayments
    {
        get
        {
            EnsureTenantDb();
            return _salaryPaymentRepository ??= new SalaryPaymentRepository(_tenantContextDb!);
        }
    }

    public IAdvanceSalaryRepository AdvanceSalaries
    {
        get
        {
            EnsureTenantDb();
            return _advanceSalaryRepository ??= new AdvanceSalaryRepository(_tenantContextDb!);
        }
    }

    public ILeaveCategoryRepository LeaveCategories
    {
        get
        {
            EnsureTenantDb();
            return _leaveCategoryRepository ??= new LeaveCategoryRepository(_tenantContextDb!);
        }
    }

    public ILeaveRequestRepository LeaveRequests
    {
        get
        {
            EnsureTenantDb();
            return _leaveRequestRepository ??= new LeaveRequestRepository(_tenantContextDb!);
        }
    }

    public IAwardRepository Awards
    {
        get
        {
            EnsureTenantDb();
            return _awardRepository ??= new AwardRepository(_tenantContextDb!);
        }
    }

    public IClassControlRepository ClassControls
    {
        get
        {
            EnsureTenantDb();
            return _classControlRepository ??= new ClassControlRepository(_tenantContextDb!);
        }
    }

    public ISectionControlRepository SectionControls
    {
        get
        {
            EnsureTenantDb();
            return _sectionControlRepository ??= new SectionControlRepository(_tenantContextDb!);
        }
    }

    public IClassTeacherRepository ClassTeachers
    {
        get
        {
            EnsureTenantDb();
            return _classTeacherRepository ??= new ClassTeacherRepository(_tenantContextDb!);
        }
    }

    public ISubjectRepository Subjects
    {
        get
        {
            EnsureTenantDb();
            return _subjectRepository ??= new SubjectRepository(_tenantContextDb!);
        }
    }

    public IClassSubjectAssignmentRepository ClassSubjectAssignments
    {
        get
        {
            EnsureTenantDb();
            return _classSubjectAssignmentRepository ??= new ClassSubjectAssignmentRepository(_tenantContextDb!);
        }
    }

    public IClassScheduleRepository ClassSchedules
    {
        get
        {
            EnsureTenantDb();
            return _classScheduleRepository ??= new ClassScheduleRepository(_tenantContextDb!);
        }
    }

    public IStudentPromotionRepository StudentPromotions
    {
        get
        {
            EnsureTenantDb();
            return _studentPromotionRepository ??= new StudentPromotionRepository(_tenantContextDb!);
        }
    }

    public IExamTermRepository ExamTerms
    {
        get
        {
            EnsureTenantDb();
            return _examTermRepository ??= new ExamTermRepository(_tenantContextDb!);
        }
    }

    public IExamHallRepository ExamHalls
    {
        get
        {
            EnsureTenantDb();
            return _examHallRepository ??= new ExamHallRepository(_tenantContextDb!);
        }
    }

    public IMarkDistributionRepository MarkDistributions
    {
        get
        {
            EnsureTenantDb();
            return _markDistributionRepository ??= new MarkDistributionRepository(_tenantContextDb!);
        }
    }

    public IExamRepository Exams
    {
        get
        {
            EnsureTenantDb();
            return _examRepository ??= new ExamRepository(_tenantContextDb!);
        }
    }

    public IExamScheduleRepository ExamSchedules
    {
        get
        {
            EnsureTenantDb();
            return _examScheduleRepository ??= new ExamScheduleRepository(_tenantContextDb!);
        }
    }

    public IMarkEntryRepository MarkEntries
    {
        get
        {
            EnsureTenantDb();
            return _markEntryRepository ??= new MarkEntryRepository(_tenantContextDb!);
        }
    }

    public IGradeRangeRepository GradeRanges
    {
        get
        {
            EnsureTenantDb();
            return _gradeRangeRepository ??= new GradeRangeRepository(_tenantContextDb!);
        }
    }

    public IExamPositionRepository ExamPositions
    {
        get
        {
            EnsureTenantDb();
            return _examPositionRepository ??= new ExamPositionRepository(_tenantContextDb!);
        }
    }

    public IStudentAttendanceRepository StudentAttendances
    {
        get
        {
            EnsureTenantDb();
            return _studentAttendanceRepository ??= new StudentAttendanceRepository(_tenantContextDb!);
        }
    }

    public IEmployeeAttendanceRepository EmployeeAttendances
    {
        get
        {
            EnsureTenantDb();
            return _employeeAttendanceRepository ??= new EmployeeAttendanceRepository(_tenantContextDb!);
        }
    }

    public IExamAttendanceRepository ExamAttendances
    {
        get
        {
            EnsureTenantDb();
            return _examAttendanceRepository ??= new ExamAttendanceRepository(_tenantContextDb!);
        }
    }

    public IBookCategoryRepository BookCategories
    {
        get
        {
            EnsureTenantDb();
            return _bookCategoryRepository ??= new BookCategoryRepository(_tenantContextDb!);
        }
    }

    public IBookRepository Books
    {
        get
        {
            EnsureTenantDb();
            return _bookRepository ??= new BookRepository(_tenantContextDb!);
        }
    }

    public IBookIssueRepository BookIssues
    {
        get
        {
            EnsureTenantDb();
            return _bookIssueRepository ??= new BookIssueRepository(_tenantContextDb!);
        }
    }

    public IEventTypeRepository EventTypes
    {
        get
        {
            EnsureTenantDb();
            return _eventTypeRepository ??= new EventTypeRepository(_tenantContextDb!);
        }
    }

    public IEventRepository Events
    {
        get
        {
            EnsureTenantDb();
            return _eventRepository ??= new EventRepository(_tenantContextDb!);
        }
    }

    public IFeesTypeRepository FeesTypes
    {
        get
        {
            EnsureTenantDb();
            return _feesTypeRepository ??= new FeesTypeRepository(_tenantContextDb!);
        }
    }

    public IFeesGroupRepository FeesGroups
    {
        get
        {
            EnsureTenantDb();
            return _feesGroupRepository ??= new FeesGroupRepository(_tenantContextDb!);
        }
    }

    public IFeesGroupItemRepository FeesGroupItems
    {
        get
        {
            EnsureTenantDb();
            return _feesGroupItemRepository ??= new FeesGroupItemRepository(_tenantContextDb!);
        }
    }

    public IFeesAllocationRepository FeesAllocations
    {
        get
        {
            EnsureTenantDb();
            return _feesAllocationRepository ??= new FeesAllocationRepository(_tenantContextDb!);
        }
    }

    public IFineSetupRepository FineSetups
    {
        get
        {
            EnsureTenantDb();
            return _fineSetupRepository ??= new FineSetupRepository(_tenantContextDb!);
        }
    }

    public IFeesReminderRepository FeesReminders
    {
        get
        {
            EnsureTenantDb();
            return _feesReminderRepository ??= new FeesReminderRepository(_tenantContextDb!);
        }
    }

    public IStudentFeeInvoiceRepository StudentFeeInvoices
    {
        get
        {
            EnsureTenantDb();
            return _studentFeeInvoiceRepository ??= new StudentFeeInvoiceRepository(_tenantContextDb!);
        }
    }

    public IOfflinePaymentRepository OfflinePayments
    {
        get
        {
            EnsureTenantDb();
            return _offlinePaymentRepository ??= new OfflinePaymentRepository(_tenantContextDb!);
        }
    }

    public IOfflinePaymentTypeRepository OfflinePaymentTypes
    {
        get
        {
            EnsureTenantDb();
            return _offlinePaymentTypeRepository ??= new OfflinePaymentTypeRepository(_tenantContextDb!);
        }
    }

    public IAccountingAccountRepository AccountingAccounts
    {
        get
        {
            EnsureTenantDb();
            return _accountingAccountRepository ??= new AccountingAccountRepository(_tenantContextDb!);
        }
    }

    public IAccountingDepositRepository AccountingDeposits
    {
        get
        {
            EnsureTenantDb();
            return _accountingDepositRepository ??= new AccountingDepositRepository(_tenantContextDb!);
        }
    }

    public IAccountingExpenseRepository AccountingExpenses
    {
        get
        {
            EnsureTenantDb();
            return _accountingExpenseRepository ??= new AccountingExpenseRepository(_tenantContextDb!);
        }
    }

    public IVoucherHeadRepository VoucherHeads
    {
        get
        {
            EnsureTenantDb();
            return _voucherHeadRepository ??= new VoucherHeadRepository(_tenantContextDb!);
        }
    }

    public IMessageRepository Messages
    {
        get
        {
            EnsureTenantDb();
            return _messageRepository ??= new MessageRepository(_tenantContextDb!);
        }
    }

    public ISchoolSettingsRepository SchoolSettings
    {
        get
        {
            EnsureTenantDb();
            return _schoolSettingsRepository ??= new SchoolSettingsRepository(_tenantContextDb!);
        }
    }

    public IGlobalSettingsRepository GlobalSettings =>
        _globalSettingsRepository ??= new GlobalSettingsRepository(_masterContext);

    public IBiometricDeviceRepository BiometricDevices
    {
        get
        {
            EnsureTenantDb();
            return _biometricDeviceRepository ??= new BiometricDeviceRepository(_tenantContextDb!);
        }
    }

    public IBiometricUserMapRepository BiometricUserMaps
    {
        get
        {
            EnsureTenantDb();
            return _biometricUserMapRepository ??= new BiometricUserMapRepository(_tenantContextDb!);
        }
    }

    public IBiometricPunchLogRepository BiometricPunchLogs
    {
        get
        {
            EnsureTenantDb();
            return _biometricPunchLogRepository ??= new BiometricPunchLogRepository(_tenantContextDb!);
        }
    }

    public IBiometricDeviceRegistryRepository BiometricDeviceRegistries =>
        _biometricDeviceRegistryRepository ??= new BiometricDeviceRegistryRepository(_masterContext);

    public IRoleRepository Roles
    {
        get
        {
            EnsureTenantDb();
            return _roleRepository ??= new RoleRepository(_tenantContextDb!);
        }
    }

    public IAcademicSessionRepository AcademicSessions
    {
        get
        {
            EnsureTenantDb();
            return _academicSessionRepository ??= new AcademicSessionRepository(_tenantContextDb!);
        }
    }

    public IDatabaseBackupRepository DatabaseBackups
    {
        get
        {
            EnsureTenantDb();
            return _databaseBackupRepository ??= new DatabaseBackupRepository(_tenantContextDb!);
        }
    }

    public ILoginLogRepository LoginLogs
    {
        get
        {
            EnsureTenantDb();
            return _loginLogRepository ??= new LoginLogRepository(_tenantContextDb!);
        }
    }

    public IEmailSettingsRepository EmailSettings
    {
        get
        {
            EnsureTenantDb();
            return _emailSettingsRepository ??= new EmailSettingsRepository(_tenantContextDb!);
        }
    }

    public ISmsSettingsRepository SmsSettings
    {
        get
        {
            EnsureTenantDb();
            return _smsSettingsRepository ??= new SmsSettingsRepository(_tenantContextDb!);
        }
    }

    public ICronSecretRegistryRepository CronSecretRegistries =>
        _cronSecretRegistryRepository ??= new CronSecretRegistryRepository(_masterContext);

    public async Task BeginTenantTransactionAsync(CancellationToken cancellationToken = default)
    {
        EnsureTenantDb();
        if (_tenantTransaction is not null)
            return;
        _tenantTransaction = await _tenantContextDb!.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTenantTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_tenantTransaction is null)
            return;
        await _tenantTransaction.CommitAsync(cancellationToken);
        await _tenantTransaction.DisposeAsync();
        _tenantTransaction = null;
    }

    public async Task RollbackTenantTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_tenantTransaction is null)
            return;
        try
        {
            await _tenantTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _tenantTransaction.DisposeAsync();
            _tenantTransaction = null;
        }
    }

    public void ClearTenantChangeTracker()
    {
        _tenantContextDb?.ChangeTracker.Clear();
    }

    public async Task<int> SaveMasterChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _masterContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveTenantChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureTenantDb();
        return await _tenantContextDb!.SaveChangesAsync(cancellationToken);
    }

    private void EnsureTenantDb()
    {
        if (_tenantContextDb is not null)
            return;

        if (string.IsNullOrEmpty(_tenantContext.SchemaName))
            throw new InvalidOperationException("Tenant context is not available for this operation.");

        _tenantContextDb = _tenantDbContextFactory.Create(_tenantContext.SchemaName);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (_tenantTransaction is not null)
            await _tenantTransaction.DisposeAsync();

        if (_tenantContextDb is not null)
            await _tenantContextDb.DisposeAsync();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
