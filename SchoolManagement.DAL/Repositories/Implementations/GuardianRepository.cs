using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class GuardianRepository : IGuardianRepository
{
    private readonly TenantDbContext _context;

    public GuardianRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<Guardian?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Guardians.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<Guardian?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Guardians
            .Include(g => g.User)
            .Include(g => g.Student)
                .ThenInclude(s => s!.Class)
            .Include(g => g.Student)
                .ThenInclude(s => s!.Section)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<Guardian?> GetByIdWithStudentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Guardians
            .Include(g => g.Student)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<Guardian?> GetPrimaryByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Guardians
            .Include(g => g.User)
            .Include(g => g.Student)
                .ThenInclude(s => s!.Class)
            .Include(g => g.Student)
                .ThenInclude(s => s!.Section)
            .Where(g => g.UserId == userId && g.IsActive)
            .OrderByDescending(g => g.ReferenceNo != null)
            .ThenByDescending(g => g.IsPrimary)
            .ThenBy(g => g.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guardian>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Guardians
            .Include(g => g.Student)
                .ThenInclude(s => s!.Class)
            .Include(g => g.Student)
                .ThenInclude(s => s!.Section)
            .Include(g => g.User)
            .Where(g => g.UserId == userId && g.IsActive)
            .OrderBy(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guardian>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Guardians
            .Where(g => g.StudentId == studentId && g.IsActive)
            .OrderByDescending(g => g.IsPrimary)
            .ThenBy(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Guardian> Items, int TotalCount)> SearchAsync(
        GuardianSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Guardians
            .Include(g => g.User)
            .Include(g => g.Student)
                .ThenInclude(s => s!.Class)
            .Include(g => g.Student)
                .ThenInclude(s => s!.Section)
            .AsQueryable();

        if (filter.IsActive.HasValue)
            query = query.Where(g => g.IsActive == filter.IsActive.Value);

        if (filter.IsLoginActive.HasValue)
        {
            query = query.Where(g =>
                (g.User != null && g.User.Active == filter.IsLoginActive.Value) ||
                (g.User == null && g.IsLoginActive == filter.IsLoginActive.Value));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(g =>
                g.Name.ToLower().Contains(term) ||
                (g.ReferenceNo != null && g.ReferenceNo.ToLower().Contains(term)) ||
                g.MobileNo.ToLower().Contains(term) ||
                (g.Email != null && g.Email.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 10_000 ? 20 : filter.PageSize;

        query = ApplySort(query, filter.SortBy, filter.SortDir);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<string> GenerateNextReferenceNoAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = year.ToString();

        var existing = await _context.Guardians
            .Where(g => g.ReferenceNo != null && g.ReferenceNo.StartsWith(prefix))
            .Select(g => g.ReferenceNo!)
            .ToListAsync(cancellationToken);

        var pending = _context.ChangeTracker.Entries<Guardian>()
            .Where(e => e.State == EntityState.Added &&
                        e.Entity.ReferenceNo != null &&
                        e.Entity.ReferenceNo.StartsWith(prefix))
            .Select(e => e.Entity.ReferenceNo!)
            .ToList();

        existing.AddRange(pending);

        var maxSeq = 0;
        foreach (var refNo in existing)
        {
            if (refNo.Length <= prefix.Length) continue;
            if (int.TryParse(refNo[prefix.Length..], out var seq) && seq > maxSeq)
                maxSeq = seq;
        }

        return $"{prefix}{(maxSeq + 1).ToString().PadLeft(3, '0')}";
    }

    public async Task BackfillMissingReferenceNosAsync(CancellationToken cancellationToken = default)
    {
        var missing = await _context.Guardians
            .Where(g => g.ReferenceNo == null)
            .OrderBy(g => g.CreatedAt)
            .ThenBy(g => g.Id)
            .ToListAsync(cancellationToken);

        foreach (var g in missing)
        {
            g.ReferenceNo = await GenerateNextReferenceNoAsync(cancellationToken);
            g.UpdatedAt = DateTime.UtcNow;
        }

        if (missing.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountActiveGuardiansForStudentAsync(
        Guid studentId,
        Guid? excludeGuardianId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Guardians.Where(g => g.StudentId == studentId && g.IsActive);
        if (excludeGuardianId.HasValue)
            query = query.Where(g => g.Id != excludeGuardianId.Value);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<Guardian> AddAsync(Guardian guardian, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(guardian.ReferenceNo))
            guardian.ReferenceNo = await GenerateNextReferenceNoAsync(cancellationToken);

        if (!guardian.IsActive)
            guardian.IsActive = true;

        await _context.Guardians.AddAsync(guardian, cancellationToken);
        return guardian;
    }

    public Task UpdateAsync(Guardian guardian, CancellationToken cancellationToken = default)
    {
        _context.Guardians.Update(guardian);
        return Task.CompletedTask;
    }

    private static IQueryable<Guardian> ApplySort(IQueryable<Guardian> query, string? sortBy, string? sortDir)
    {
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        var key = (sortBy ?? "createdat").Trim().ToLowerInvariant();

        return key switch
        {
            "name" or "guardianname" => desc
                ? query.OrderByDescending(g => g.Name)
                : query.OrderBy(g => g.Name),
            "occupation" => desc
                ? query.OrderByDescending(g => g.Occupation)
                : query.OrderBy(g => g.Occupation),
            "referenceno" or "mobileno" => desc
                ? query.OrderByDescending(g => g.ReferenceNo)
                : query.OrderBy(g => g.ReferenceNo),
            "email" => desc
                ? query.OrderByDescending(g => g.Email)
                : query.OrderBy(g => g.Email),
            _ => desc
                ? query.OrderByDescending(g => g.CreatedAt)
                : query.OrderBy(g => g.CreatedAt)
        };
    }
}
