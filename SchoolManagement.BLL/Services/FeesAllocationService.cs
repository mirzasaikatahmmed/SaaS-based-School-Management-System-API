using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.StudentAccounting;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class FeesAllocationService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IHttpContextAccessor http) : IFeesAllocationService
{
    public async Task<IReadOnlyList<FeesAllocationResponseDto>> GetFilteredAsync(FeesAllocationFilterDto filter, CancellationToken ct = default)
    {
        await Ready(ct);
        var items = await uow.FeesAllocations.GetFilteredAsync(new FeesAllocationFilter
        {
            ClassId = filter.ClassId,
            SectionId = filter.SectionId,
            AcademicYear = filter.AcademicYear,
            IsActive = filter.IsActive
        }, ct);

        var result = new List<FeesAllocationResponseDto>();
        foreach (var a in items)
            result.Add(await MapAsync(a, ct));
        return result;
    }

    public async Task<FeesAllocationResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        var a = await uow.FeesAllocations.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees allocation '{id}' not found.");
        return await MapAsync(a, ct);
    }

    public async Task<FeesAllocationResponseDto> CreateAsync(CreateFeesAllocationDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();

        var group = await uow.FeesGroups.GetByIdAsync(dto.FeesGroupId, ct)
            ?? throw new NotFoundException($"Fees group '{dto.FeesGroupId}' not found.");
        if (!group.IsActive)
            throw new AppException($"Fees group '{group.Name}' is inactive.", 400);
        if (dto.AcademicYear < 2000)
            throw new AppException("A valid academic year is required.", 400);
        if (await uow.FeesAllocations.ExistsUniqueAsync(dto.ClassId, dto.SectionId, dto.FeesGroupId, dto.AcademicYear, null, ct))
            throw new ConflictException("A fees allocation already exists for this class, section, group, and academic year.");

        var allocation = new FeesAllocation
        {
            Id = Guid.NewGuid(),
            ClassId = dto.ClassId,
            SectionId = dto.SectionId,
            FeesGroupId = dto.FeesGroupId,
            AcademicYear = dto.AcademicYear,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await uow.FeesAllocations.AddAsync(allocation, ct);
        await uow.SaveTenantChangesAsync(ct);

        var loaded = await uow.FeesAllocations.GetByIdAsync(allocation.Id, ct) ?? allocation;
        await GenerateForAllocationAsync(loaded, ct);

        return await MapAsync(await uow.FeesAllocations.GetByIdAsync(allocation.Id, ct) ?? loaded, ct);
    }

    public async Task<FeesAllocationResponseDto> UpdateAsync(Guid id, UpdateFeesAllocationDto dto, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var a = await uow.FeesAllocations.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees allocation '{id}' not found.");
        if (dto.IsActive.HasValue) a.IsActive = dto.IsActive.Value;
        await uow.FeesAllocations.UpdateAsync(a, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapAsync(a, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var a = await uow.FeesAllocations.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Fees allocation '{id}' not found.");
        var invoices = await uow.StudentFeeInvoices.GetFilteredAsync(new StudentFeeInvoiceFilter { FeesAllocationId = id, PageSize = 1 }, ct);
        if (invoices.TotalCount > 0)
            throw new AppException("Cannot delete an allocation that already has generated invoices.", 400);
        await uow.FeesAllocations.DeleteAsync(a, ct);
        await uow.SaveTenantChangesAsync(ct);
    }

    public async Task<GenerateInvoicesResultDto> GenerateInvoicesForAllocationAsync(Guid allocationId, CancellationToken ct = default)
    {
        await Ready(ct);
        Manage();
        var allocation = await uow.FeesAllocations.GetByIdAsync(allocationId, ct)
            ?? throw new NotFoundException($"Fees allocation '{allocationId}' not found.");
        return await GenerateForAllocationAsync(allocation, ct);
    }

    private async Task<GenerateInvoicesResultDto> GenerateForAllocationAsync(FeesAllocation allocation, CancellationToken ct)
    {
        var totalAmount = allocation.FeesGroup.Items.Sum(i => i.Amount);
        var (students, _) = await uow.Students.SearchAsync(new StudentSearchFilter
        {
            ClassId = allocation.ClassId,
            SectionId = allocation.SectionId,
            IsActive = true,
            Page = 1,
            PageSize = 10_000
        }, ct);

        var generated = 0;
        var skipped = 0;
        foreach (var student in students)
        {
            var existing = await uow.StudentFeeInvoices.GetByStudentAndAllocationAsync(student.Id, allocation.Id, ct);
            if (existing is not null)
            {
                skipped++;
                continue;
            }

            var invoice = new StudentFeeInvoice
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                FeesAllocationId = allocation.Id,
                FeesGroupId = allocation.FeesGroupId,
                ClassId = allocation.ClassId,
                SectionId = allocation.SectionId,
                TotalAmount = totalAmount,
                PaidAmount = 0,
                FineAmount = 0,
                DueAmount = totalAmount,
                Status = FeeInvoiceStatuses.Unpaid,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.StudentFeeInvoices.AddAsync(invoice, ct);
            generated++;
        }

        await uow.SaveTenantChangesAsync(ct);
        return new GenerateInvoicesResultDto { Generated = generated, Skipped = skipped, TotalStudents = students.Count };
    }

    private async Task<FeesAllocationResponseDto> MapAsync(FeesAllocation a, CancellationToken ct)
    {
        var invoices = await uow.StudentFeeInvoices.GetFilteredAsync(new StudentFeeInvoiceFilter { FeesAllocationId = a.Id, PageSize = 1 }, ct);
        return new FeesAllocationResponseDto
        {
            Id = a.Id,
            ClassId = a.ClassId,
            ClassName = a.Class?.Name ?? string.Empty,
            SectionId = a.SectionId,
            SectionName = a.Section?.Name ?? string.Empty,
            FeesGroupId = a.FeesGroupId,
            FeesGroupName = a.FeesGroup?.Name ?? string.Empty,
            TotalAmount = a.FeesGroup?.Items.Sum(i => i.Amount) ?? 0,
            AcademicYear = a.AcademicYear,
            IsActive = a.IsActive,
            InvoicesGenerated = invoices.TotalCount,
            CreatedAt = a.CreatedAt
        };
    }

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureStudentAndOfficeAccountingModuleAsync(tenant.SchemaName!, ct);
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
        if (!r.Contains(AppConstants.Roles.Admin) && !r.Contains(AppConstants.Roles.SuperAdmin) && !r.Contains(AppConstants.Roles.Accountant))
            throw new ForbiddenException("Only Super Admin, School Admin, or Accountant can manage fees allocations.");
    }
}
