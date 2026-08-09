using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Library;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class BookService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IStorageService storage,
    IHttpContextAccessor http) : IBookService
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public async Task<BookListResponseDto> GetListAsync(BookFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 200 ? 25 : filter.PageSize;
        var (items, total) = await uow.Books.SearchAsync(new BookSearchFilter
        {
            CategoryId = filter.CategoryId,
            Search = filter.Search,
            Page = page,
            PageSize = size
        }, ct);

        var data = new List<BookListItemDto>();
        var i = 0;
        foreach (var b in items)
            data.Add(await MapList(b, (page - 1) * size + ++i, ct));

        return new BookListResponseDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<BookDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var book = await uow.Books.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Book '{id}' not found.");
        return await MapDetail(book, ct);
    }

    public async Task<BookDetailDto> CreateAsync(CreateBookDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var title = dto.Title.Trim();
        if (await uow.Books.TitleExistsAsync(title, null, ct))
            throw new ConflictException($"Book '{title}' already exists.");
        if (dto.TotalStock < 0)
            throw new AppException("Total stock cannot be negative.", 400);

        if (dto.CategoryId.HasValue && await uow.BookCategories.GetByIdAsync(dto.CategoryId.Value, ct) is null)
            throw new NotFoundException("Book category not found.");

        var entity = new Book
        {
            Id = Guid.NewGuid(),
            Title = title,
            IsbnNo = dto.IsbnNo?.Trim(),
            Author = dto.Author?.Trim(),
            Edition = dto.Edition?.Trim(),
            Publisher = dto.Publisher?.Trim(),
            PurchaseDate = dto.PurchaseDate,
            CategoryId = dto.CategoryId,
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            TotalStock = dto.TotalStock,
            IssuedCopies = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await uow.Books.AddAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(await uow.Books.GetByIdAsync(entity.Id, ct) ?? entity, ct);
    }

    public async Task<BookDetailDto> UpdateAsync(Guid id, UpdateBookDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var entity = await uow.Books.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Book '{id}' not found.");

        var title = dto.Title.Trim();
        if (await uow.Books.TitleExistsAsync(title, id, ct))
            throw new ConflictException($"Book '{title}' already exists.");
        if (dto.TotalStock < entity.IssuedCopies)
            throw new AppException($"Total stock cannot be less than currently issued copies ({entity.IssuedCopies}).", 400);

        if (dto.CategoryId.HasValue && await uow.BookCategories.GetByIdAsync(dto.CategoryId.Value, ct) is null)
            throw new NotFoundException("Book category not found.");

        entity.Title = title;
        entity.IsbnNo = dto.IsbnNo?.Trim();
        entity.Author = dto.Author?.Trim();
        entity.Edition = dto.Edition?.Trim();
        entity.Publisher = dto.Publisher?.Trim();
        entity.PurchaseDate = dto.PurchaseDate;
        entity.CategoryId = dto.CategoryId;
        entity.Description = dto.Description?.Trim();
        entity.Price = dto.Price;
        entity.TotalStock = dto.TotalStock;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;

        await uow.Books.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(await uow.Books.GetByIdAsync(id, ct) ?? entity, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var entity = await uow.Books.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Book '{id}' not found.");
        if (entity.IssuedCopies > 0)
            throw new AppException($"Book has {entity.IssuedCopies} copy(ies) currently issued and cannot be deleted.", 400);

        await uow.Books.DeleteAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<BookDetailDto> UploadCoverAsync(Guid id, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext) || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
            (stream.CanSeek && stream.Length > 2 * 1024 * 1024))
            throw new AppException("Only jpg, jpeg, png, and webp images up to 2MB are allowed.", 400);

        var entity = await uow.Books.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Book '{id}' not found.");
        var slug = tenant.TenantSlug ?? throw new AppException("Tenant slug is not resolved.", 400);

        if (!string.IsNullOrWhiteSpace(entity.CoverImageUrl))
        {
            try { await storage.DeleteFileAsync(slug, entity.CoverImageUrl, ct); } catch { /* best effort */ }
        }

        var key = $"{AppConstants.StorageFolders.LibraryCovers}/{id}{ext}";
        await storage.UploadObjectAsync(slug, key, stream, contentType, ct);
        entity.CoverImageUrl = key;
        entity.UpdatedAt = DateTime.UtcNow;

        await uow.Books.UpdateAsync(entity, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(entity, ct);
    }

    public async Task<IReadOnlyList<BookLookupDto>> GetLookupAsync(CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var books = await uow.Books.GetLookupAsync(ct);
        return books.Select(b => new BookLookupDto
        {
            Id = b.Id,
            Title = b.Title,
            AvailableCopies = Math.Max(0, b.TotalStock - b.IssuedCopies)
        }).ToList();
    }

    private async Task<BookListItemDto> MapList(Book b, int sl, CancellationToken ct) => new()
    {
        Id = b.Id,
        Sl = sl,
        Branch = tenant.TenantName ?? string.Empty,
        Title = b.Title,
        IsbnNo = b.IsbnNo,
        Author = b.Author,
        CategoryName = b.Category?.Name,
        CoverImageUrl = await Presign(b.CoverImageUrl, ct),
        TotalStock = b.TotalStock,
        IssuedCopies = b.IssuedCopies,
        AvailableCopies = Math.Max(0, b.TotalStock - b.IssuedCopies),
        IsActive = b.IsActive
    };

    private async Task<BookDetailDto> MapDetail(Book b, CancellationToken ct)
    {
        var list = await MapList(b, 0, ct);
        return new BookDetailDto
        {
            Id = list.Id,
            Sl = list.Sl,
            Branch = list.Branch,
            Title = list.Title,
            IsbnNo = list.IsbnNo,
            Author = list.Author,
            CategoryName = list.CategoryName,
            CoverImageUrl = list.CoverImageUrl,
            TotalStock = list.TotalStock,
            IssuedCopies = list.IssuedCopies,
            AvailableCopies = list.AvailableCopies,
            IsActive = list.IsActive,
            CategoryId = b.CategoryId,
            Edition = b.Edition,
            Publisher = b.Publisher,
            PurchaseDate = b.PurchaseDate,
            Description = b.Description,
            Price = b.Price,
            CreatedAt = b.CreatedAt
        };
    }

    private async Task<string?> Presign(string? key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(tenant.TenantSlug)) return key;
        try { return await storage.GetPresignedUrlAsync(tenant.TenantSlug, key, ct); }
        catch { return key; }
    }

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
            throw new ForbiddenException("Only Super Admin, School Admin, or Librarian can manage books.");
    }
}
