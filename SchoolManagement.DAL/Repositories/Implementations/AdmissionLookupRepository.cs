using Microsoft.EntityFrameworkCore;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;

namespace SchoolManagement.DAL.Repositories.Implementations;

public class AdmissionLookupRepository : IAdmissionLookupRepository
{
    private readonly TenantDbContext _context;

    public AdmissionLookupRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ClassEntity>> GetClassesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Classes
            .Where(c => c.IsActive)
            .OrderBy(c => c.NumericName)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClassEntity?> GetClassByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Classes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Section>> GetSectionsByClassIdAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        return await _context.Sections
            .Where(s => s.IsActive && (s.ClassId == classId || s.ClassSections.Any(cs => cs.ClassId == classId)))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Section?> GetSectionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sections.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StudentCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentCategory?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StudentCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TransportRoute>> GetTransportRoutesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TransportRoutes
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<TransportRoute?> GetTransportRouteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TransportRoutes.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Hostel>> GetHostelsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Hostels
            .Where(h => h.IsActive)
            .OrderBy(h => h.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Hostel?> GetHostelByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Hostels.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<HostelRoom>> GetHostelRoomsAsync(Guid hostelId, CancellationToken cancellationToken = default)
    {
        return await _context.HostelRooms
            .Where(r => r.HostelId == hostelId && r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<HostelRoom?> GetHostelRoomByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.HostelRooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}
