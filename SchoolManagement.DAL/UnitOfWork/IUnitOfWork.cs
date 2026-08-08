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
    Task BeginTenantTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTenantTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTenantTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveMasterChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveTenantChangesAsync(CancellationToken cancellationToken = default);
}
