using SchoolManagement.BLL.DTOs.Biometric;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class BiometricUserMapService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner) : IBiometricUserMapService
{
    public async Task<IReadOnlyList<UserMapResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        return (await uow.BiometricUserMaps.GetAllAsync(cancellationToken)).Select(Map).ToList();
    }

    public async Task<UserMapResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        var map = await uow.BiometricUserMaps.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Mapping '{id}' not found.");
        return Map(map);
    }

    public async Task<UserMapResponseDto> CreateAsync(CreateUserMapDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        Validate(dto.PersonType, dto.StudentId, dto.EmployeeId);

        if (string.IsNullOrWhiteSpace(dto.DevicePin))
            throw new AppException("DevicePin is required.", 400);

        var pin = dto.DevicePin.Trim();
        if (await uow.BiometricUserMaps.PinExistsAsync(pin, null, cancellationToken))
            throw new ConflictException($"Device PIN '{pin}' is already mapped.");

        if (string.Equals(dto.PersonType, "Student", StringComparison.OrdinalIgnoreCase))
        {
            _ = await uow.Students.GetByIdAsync(dto.StudentId!.Value, cancellationToken)
                ?? throw new NotFoundException($"Student '{dto.StudentId}' not found.");
        }
        else
        {
            _ = await uow.Employees.GetByIdAsync(dto.EmployeeId!.Value, cancellationToken)
                ?? throw new NotFoundException($"Employee '{dto.EmployeeId}' not found.");
        }

        var map = new BiometricUserMap
        {
            Id = Guid.NewGuid(),
            DevicePin = pin,
            PersonType = string.Equals(dto.PersonType, "Employee", StringComparison.OrdinalIgnoreCase) ? "Employee" : "Student",
            StudentId = dto.StudentId,
            EmployeeId = dto.EmployeeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.BiometricUserMaps.AddAsync(map, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);

        var saved = await uow.BiometricUserMaps.GetByIdAsync(map.Id, cancellationToken) ?? map;
        return Map(saved);
    }

    public async Task<UserMapResponseDto> UpdateAsync(Guid id, UpdateUserMapDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        var map = await uow.BiometricUserMaps.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Mapping '{id}' not found.");

        if (!string.IsNullOrWhiteSpace(dto.DevicePin) && dto.DevicePin.Trim() != map.DevicePin)
        {
            var pin = dto.DevicePin.Trim();
            if (await uow.BiometricUserMaps.PinExistsAsync(pin, id, cancellationToken))
                throw new ConflictException($"Device PIN '{pin}' is already mapped.");
            map.DevicePin = pin;
        }
        if (dto.IsActive.HasValue) map.IsActive = dto.IsActive.Value;
        map.UpdatedAt = DateTime.UtcNow;

        await uow.BiometricUserMaps.UpdateAsync(map, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);

        var saved = await uow.BiometricUserMaps.GetByIdAsync(map.Id, cancellationToken) ?? map;
        return Map(saved);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        var map = await uow.BiometricUserMaps.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Mapping '{id}' not found.");
        await uow.BiometricUserMaps.DeleteAsync(map, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);
    }

    private static void Validate(string? personType, Guid? studentId, Guid? employeeId)
    {
        var isStudent = string.Equals(personType, "Student", StringComparison.OrdinalIgnoreCase);
        var isEmployee = string.Equals(personType, "Employee", StringComparison.OrdinalIgnoreCase);
        if (!isStudent && !isEmployee)
            throw new AppException("PersonType must be 'Student' or 'Employee'.", 400);

        var hasStudent = studentId.HasValue && studentId.Value != Guid.Empty;
        var hasEmployee = employeeId.HasValue && employeeId.Value != Guid.Empty;

        if (hasStudent == hasEmployee)
            throw new AppException("Exactly one of StudentId or EmployeeId must be provided.", 400);
        if (isStudent && !hasStudent)
            throw new AppException("StudentId is required when PersonType is 'Student'.", 400);
        if (isEmployee && !hasEmployee)
            throw new AppException("EmployeeId is required when PersonType is 'Employee'.", 400);
    }

    private static UserMapResponseDto Map(BiometricUserMap m) => new()
    {
        Id = m.Id,
        DevicePin = m.DevicePin,
        PersonType = m.PersonType,
        StudentId = m.StudentId,
        StudentName = m.Student is null ? null : $"{m.Student.FirstName} {m.Student.LastName}".Trim(),
        StudentRegisterNo = m.Student?.RegisterNo,
        EmployeeId = m.EmployeeId,
        EmployeeName = m.Employee?.Name,
        IsActive = m.IsActive,
        CreatedAt = m.CreatedAt
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName))
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureBiometricModuleAsync(tenant.SchemaName!, ct);
    }
}
