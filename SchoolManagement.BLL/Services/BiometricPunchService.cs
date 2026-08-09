using SchoolManagement.BLL.DTOs.Biometric;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class BiometricPunchService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner,
    IBiometricPunchProcessor processor) : IBiometricPunchService
{
    public async Task<PunchLogListResponseDto> GetPunchesAsync(PunchLogFilterDto filter, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var size = filter.PageSize is < 1 or > 500 ? 50 : filter.PageSize;

        var from = filter.From.HasValue ? DateTime.SpecifyKind(filter.From.Value, DateTimeKind.Utc) : (DateTime?)null;
        var to = filter.To.HasValue ? DateTime.SpecifyKind(filter.To.Value, DateTimeKind.Utc) : (DateTime?)null;
        if (to.HasValue && to.Value.TimeOfDay == TimeSpan.Zero)
            to = to.Value.Date.AddDays(1).AddTicks(-1);

        var (items, total) = await uow.BiometricPunchLogs.GetFilteredAsync(
            from, to, filter.DeviceId, filter.Kind, page, size, cancellationToken);

        return new PunchLogListResponseDto
        {
            Data = items.Select(Map).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = size,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<PunchLogItemDto> RecordManualPunchAsync(ManualPunchDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);

        if (string.IsNullOrWhiteSpace(dto.SerialNumber))
            throw new AppException("SerialNumber is required.", 400);
        if (string.IsNullOrWhiteSpace(dto.DevicePin))
            throw new AppException("DevicePin is required.", 400);

        var device = await uow.BiometricDevices.GetBySerialNumberAsync(dto.SerialNumber.Trim(), cancellationToken)
            ?? throw new NotFoundException($"Device with serial number '{dto.SerialNumber}' not found.");

        var punchTime = dto.PunchTime.HasValue
            ? DateTime.SpecifyKind(dto.PunchTime.Value, DateTimeKind.Utc)
            : DateTime.UtcNow;

        var log = await processor.ProcessPunchAsync(
            device.Id, device.SerialNumber, device.ExamGraceMinutesBefore, device.ExamGraceMinutesAfter,
            dto.DevicePin.Trim(), punchTime, "manual-test", cancellationToken);

        return await MapWithNamesAsync(log, cancellationToken);
    }

    private async Task<PunchLogItemDto> MapWithNamesAsync(BiometricPunchLog log, CancellationToken ct)
    {
        string? studentName = null, employeeName = null;
        if (log.StudentId.HasValue)
        {
            var s = await uow.Students.GetByIdAsync(log.StudentId.Value, ct);
            studentName = s is null ? null : $"{s.FirstName} {s.LastName}".Trim();
        }
        if (log.EmployeeId.HasValue)
        {
            var e = await uow.Employees.GetByIdAsync(log.EmployeeId.Value, ct);
            employeeName = e?.Name;
        }

        return new PunchLogItemDto
        {
            Id = log.Id,
            DeviceId = log.DeviceId,
            DeviceSn = log.DeviceSn,
            DevicePin = log.DevicePin,
            PunchTime = log.PunchTime,
            PunchKind = log.PunchKind,
            StatusApplied = log.StatusApplied,
            StudentId = log.StudentId,
            StudentName = studentName,
            EmployeeId = log.EmployeeId,
            EmployeeName = employeeName,
            ExamId = log.ExamId,
            SubjectId = log.SubjectId,
            RawLine = log.RawLine,
            CreatedAt = log.CreatedAt
        };
    }

    private static PunchLogItemDto Map(BiometricPunchLog p) => new()
    {
        Id = p.Id,
        DeviceId = p.DeviceId,
        DeviceSn = p.DeviceSn,
        DeviceName = p.Device?.Name,
        DevicePin = p.DevicePin,
        PunchTime = p.PunchTime,
        PunchKind = p.PunchKind,
        StatusApplied = p.StatusApplied,
        StudentId = p.StudentId,
        StudentName = p.Student is null ? null : $"{p.Student.FirstName} {p.Student.LastName}".Trim(),
        EmployeeId = p.EmployeeId,
        EmployeeName = p.Employee?.Name,
        ExamId = p.ExamId,
        SubjectId = p.SubjectId,
        RawLine = p.RawLine,
        CreatedAt = p.CreatedAt
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureBiometricModuleAsync(tenant.SchemaName!, ct);
    }
}
