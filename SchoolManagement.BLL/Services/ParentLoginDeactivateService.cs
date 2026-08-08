using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Parents;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class ParentLoginDeactivateService : IParentLoginDeactivateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantSchemaProvisioner _schemaProvisioner;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ParentLoginDeactivateService(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ITenantSchemaProvisioner schemaProvisioner,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _schemaProvisioner = schemaProvisioner;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ParentLoginDeactivateListResponseDto> GetListAsync(
        ParentLoginDeactivateFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize;

        var (items, total) = await _unitOfWork.Guardians.SearchAsync(new GuardianSearchFilter
        {
            Search = filter.Search,
            IsActive = true,
            IsLoginActive = false,
            SortBy = "name",
            SortDir = "asc",
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        var data = new List<ParentLoginDeactivateListItemDto>(items.Count);
        foreach (var g in items)
            data.Add(await MapItemAsync(g, cancellationToken));

        return new ParentLoginDeactivateListResponseDto
        {
            Data = data,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        => SetLoginActiveAsync(id, true, cancellationToken);

    public Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        => SetLoginActiveAsync(id, false, cancellationToken);

    public async Task<BulkParentLoginActivateResultDto> BulkActivateAsync(
        BulkParentLoginActivateDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        if (dto.ParentIds is null || dto.ParentIds.Count == 0)
            throw new AppException("At least one parent id is required.", 400);

        var ids = dto.ParentIds.Distinct().ToList();
        var guardians = new List<Guardian>();
        foreach (var id in ids)
        {
            var g = await _unitOfWork.Guardians.GetByIdAsync(id, cancellationToken)
                ?? throw new ForbiddenException("One or more parents do not belong to the current tenant.");
            guardians.Add(g);
        }

        await _unitOfWork.BeginTenantTransactionAsync(cancellationToken);
        try
        {
            foreach (var guardian in guardians)
            {
                await ApplyLoginActiveAsync(guardian, true, cancellationToken);
            }

            await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
            await _unitOfWork.CommitTenantTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTenantTransactionAsync(cancellationToken);
            throw;
        }

        return new BulkParentLoginActivateResultDto { Activated = guardians.Count, Failed = 0 };
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(
        ParentLoginDeactivateFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var (items, _) = await _unitOfWork.Guardians.SearchAsync(new GuardianSearchFilter
        {
            Search = filter.Search,
            IsActive = true,
            IsLoginActive = false,
            SortBy = "name",
            SortDir = "asc",
            Page = 1,
            PageSize = 10_000
        }, cancellationToken);

        var data = new List<ParentLoginDeactivateListItemDto>(items.Count);
        foreach (var g in items)
            data.Add(await MapItemAsync(g, cancellationToken));

        var fmt = (filter.Export ?? "csv").Trim().ToLowerInvariant();
        return fmt switch
        {
            "csv" or "excel" => BuildCsvExport(data, fmt == "excel"),
            "pdf" => BuildPdfExport(data),
            _ => throw new AppException("Unsupported export format. Use csv, excel, or pdf.", 400)
        };
    }

    private async Task SetLoginActiveAsync(Guid id, bool active, CancellationToken cancellationToken)
    {
        EnsureTenant();
        EnsureCanManage();
        await EnsureReadyAsync(cancellationToken);

        var guardian = await _unitOfWork.Guardians.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Parent '{id}' not found.");

        await ApplyLoginActiveAsync(guardian, active, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    private async Task ApplyLoginActiveAsync(Guardian guardian, bool active, CancellationToken cancellationToken)
    {
        guardian.IsLoginActive = active;
        guardian.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Guardians.UpdateAsync(guardian, cancellationToken);

        if (guardian.UserId.HasValue)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(guardian.UserId.Value, cancellationToken);
            if (user is not null)
            {
                user.Active = active;
                user.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            }
        }
    }

    private Task<ParentLoginDeactivateListItemDto> MapItemAsync(Guardian g, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ParentLoginDeactivateListItemDto
        {
            Id = g.Id,
            GuardianName = g.Name,
            Occupation = g.Occupation,
            MobileNo = g.MobileNo,
            Email = g.Email,
            IsLoginActive = g.User?.Active ?? g.IsLoginActive
        });
    }

    private static (byte[] Content, string ContentType, string FileName) BuildCsvExport(
        List<ParentLoginDeactivateListItemDto> items, bool asExcel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("GuardianName,Occupation,MobileNo,Email,IsLoginActive");
        foreach (var p in items)
        {
            sb.AppendLine(string.Join(',',
                Csv(p.GuardianName),
                Csv(p.Occupation),
                Csv(p.MobileNo),
                Csv(p.Email),
                p.IsLoginActive ? "Yes" : "No"));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return asExcel
            ? (bytes, "application/vnd.ms-excel", $"parent-login-deactivate-{DateTime.UtcNow:yyyyMMdd}.xls")
            : (bytes, "text/csv", $"parent-login-deactivate-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private (byte[] Content, string ContentType, string FileName) BuildPdfExport(
        List<ParentLoginDeactivateListItemDto> items)
    {
        var school = _tenantContext.TenantName ?? "School";
        var lines = new List<string> { school, "Parent Login Deactivate Export", "" };
        foreach (var p in items)
            lines.Add($"{p.GuardianName} | {p.Occupation} | {p.MobileNo} | {p.Email}");

        var content = string.Join('\n', lines)
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");

        var pdf = $"""
            %PDF-1.4
            1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj
            2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj
            3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources<< /Font<< /F1 5 0 R >> >> >>endobj
            4 0 obj<< /Length {content.Length + 50} >>stream
            BT /F1 10 Tf 40 750 Td 14 TL ({content.Replace("\n", ") Tj T* (")}) Tj ET
            endstream endobj
            5 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj
            xref
            0 6
            0000000000 65535 f 
            trailer<< /Size 6 /Root 1 0 R >>
            startxref
            0
            %%EOF
            """;

        return (Encoding.ASCII.GetBytes(pdf), "application/pdf", $"parent-login-deactivate-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    private async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        await _schemaProvisioner.EnsureAdmissionModuleAsync(_tenantContext.SchemaName!, cancellationToken);
        await _schemaProvisioner.EnsureGuardianParentFieldsAsync(_tenantContext.SchemaName!, cancellationToken);
        await _schemaProvisioner.EnsureGuardianSocialAndAlternativeFieldsAsync(_tenantContext.SchemaName!, cancellationToken);
    }

    private void EnsureTenant()
    {
        if (string.IsNullOrEmpty(_tenantContext.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
    }

    private void EnsureCanManage()
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) || roles.Contains(AppConstants.Roles.Admin))
            return;
        throw new ForbiddenException("Only Super Admin or School Admin can manage parent login.");
    }

    private HashSet<string> GetCurrentRoles()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var roles = user.FindAll("role")
            .Concat(user.FindAll(ClaimTypes.Role))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rolesCsv = user.FindFirst(AppConstants.Claims.Roles)?.Value;
        if (!string.IsNullOrWhiteSpace(rolesCsv))
        {
            foreach (var r in rolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                roles.Add(r);
        }

        return roles;
    }
}
