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
