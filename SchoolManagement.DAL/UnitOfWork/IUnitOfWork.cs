using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    ITenantRepository Tenants { get; }
    ISchoolRepository Schools { get; }
    IUserRepository Users { get; }
    IStudentRepository Students { get; }
    IGuardianRepository Guardians { get; }
    IAdmissionLookupRepository AdmissionLookups { get; }
    IOnlineAdmissionRepository OnlineAdmissions { get; }
    IImportRepository Imports { get; }
    IStudentCategoryRepository StudentCategories { get; }
    IDeactivateReasonRepository DeactivateReasons { get; }
    IDepartmentRepository Departments { get; }
    IDesignationRepository Designations { get; }
    IEmployeeRepository Employees { get; }
    ISalaryTemplateRepository SalaryTemplates { get; }
    ISalaryAssignmentRepository SalaryAssignments { get; }
    ISalaryPaymentRepository SalaryPayments { get; }
    IAdvanceSalaryRepository AdvanceSalaries { get; }
    ILeaveCategoryRepository LeaveCategories { get; }
    ILeaveRequestRepository LeaveRequests { get; }
    IAwardRepository Awards { get; }
    IClassControlRepository ClassControls { get; }
    ISectionControlRepository SectionControls { get; }
    IClassTeacherRepository ClassTeachers { get; }
    ISubjectRepository Subjects { get; }
    IClassSubjectAssignmentRepository ClassSubjectAssignments { get; }
    IClassScheduleRepository ClassSchedules { get; }
    IStudentPromotionRepository StudentPromotions { get; }
    IExamTermRepository ExamTerms { get; }
    IExamHallRepository ExamHalls { get; }
    IMarkDistributionRepository MarkDistributions { get; }
    IExamRepository Exams { get; }
    IExamScheduleRepository ExamSchedules { get; }
    IMarkEntryRepository MarkEntries { get; }
    Task BeginTenantTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTenantTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTenantTransactionAsync(CancellationToken cancellationToken = default);
    void ClearTenantChangeTracker();
    Task<int> SaveMasterChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveTenantChangesAsync(CancellationToken cancellationToken = default);
}
