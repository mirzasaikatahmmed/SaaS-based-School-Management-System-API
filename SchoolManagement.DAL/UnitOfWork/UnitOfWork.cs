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
