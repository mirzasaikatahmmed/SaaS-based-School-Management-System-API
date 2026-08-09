using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Library;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class BookCategoryService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IBookCategoryService
{
    public async Task<IReadOnlyList<BookCategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var items = await uow.BookCategories.GetAllAsync(ct);
        var result = new List<BookCategoryDto>();
        foreach (var c in items)
        {
            var count = await uow.BookCategories.CountBooksUsingAsync(c.Id, ct);
            result.Add(Map(c, count));
        }
        return result;
    }

    public async Task<BookCategoryDto> CreateAsync(CreateBookCategoryDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var name = dto.Name.Trim();
        if (await uow.BookCategories.NameExistsAsync(name, null, ct))
            throw new ConflictException($"Category '{name}' already exists.");

        var entity = new BookCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await uow.BookCategories.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return Map(entity, 0);
    }

    public async Task<BookCategoryDto> UpdateAsync(Guid id, UpdateBookCategoryDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var entity = await uow.BookCategories.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Book category '{id}' not found.");

        var name = dto.Name.Trim();
        if (await uow.BookCategories.NameExistsAsync(name, id, ct))
            throw new ConflictException($"Category '{name}' already exists.");

        entity.Name = name;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;

        await uow.BookCategories.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        var count = await uow.BookCategories.CountBooksUsingAsync(id, ct);
        return Map(entity, count);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var entity = await uow.BookCategories.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Book category '{id}' not found.");

        var count = await uow.BookCategories.CountBooksUsingAsync(id, ct);
        if (count > 0)
            throw new AppException($"Category is in use by {count} book(s) and cannot be deleted.", 400);

        await uow.BookCategories.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    private BookCategoryDto Map(BookCategory c, int count) => new()
    {
        Id = c.Id,
        Branch = tenant.TenantName ?? string.Empty,
        Name = c.Name,
        IsActive = c.IsActive,
        BookCount = count,
        CreatedAt = c.CreatedAt
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureGradesAttendanceLibraryEventsModuleAsync(tenant.SchemaName!, ct);
    }

    private HashSet<string> Roles()
    {
        var p = http.HttpContext?.User;
        if (p is null) return [];
        return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void Manage()
    {
        var r = Roles();
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) &&
            !r.Contains(AppConstants.Roles.Librarian))
            throw new ForbiddenException("Only Super Admin, School Admin, or Librarian can manage book categories.");
    }
}
