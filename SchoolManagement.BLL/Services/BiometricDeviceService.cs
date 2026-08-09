using SchoolManagement.BLL.DTOs.Biometric;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Master;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;

public class BiometricDeviceService(
    IUnitOfWork uow,
    ITenantContext tenant,
    ITenantSchemaProvisioner provisioner) : IBiometricDeviceService
{
    public async Task<IReadOnlyList<DeviceResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        return (await uow.BiometricDevices.GetAllAsync(cancellationToken)).Select(Map).ToList();
    }

    public async Task<DeviceResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);
        var device = await uow.BiometricDevices.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Device '{id}' not found.");
        return Map(device);
    }

    public async Task<DeviceResponseDto> CreateAsync(CreateDeviceDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new AppException("Name is required.", 400);
        if (string.IsNullOrWhiteSpace(dto.SerialNumber))
            throw new AppException("SerialNumber is required.", 400);

        var serialNumber = dto.SerialNumber.Trim();

        if (await uow.BiometricDevices.SerialNumberExistsAsync(serialNumber, null, cancellationToken))
            throw new ConflictException($"Device with serial number '{serialNumber}' already exists.");
        if (await uow.BiometricDeviceRegistries.SerialNumberExistsAsync(serialNumber, cancellationToken))
            throw new ConflictException($"Serial number '{serialNumber}' is already registered to another school.");

        var device = new BiometricDevice
        {
            Id = Guid.NewGuid(),
            SerialNumber = serialNumber,
            Name = dto.Name.Trim(),
            Location = dto.Location,
            DeviceModel = string.IsNullOrWhiteSpace(dto.DeviceModel) ? "K40-H" : dto.DeviceModel.Trim(),
            ExamGraceMinutesBefore = dto.ExamGraceMinutesBefore <= 0 ? 30 : dto.ExamGraceMinutesBefore,
            ExamGraceMinutesAfter = dto.ExamGraceMinutesAfter <= 0 ? 30 : dto.ExamGraceMinutesAfter,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.BiometricDevices.AddAsync(device, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);

        var registry = new BiometricDeviceRegistry
        {
            Id = Guid.NewGuid(),
            SerialNumber = serialNumber,
            TenantId = tenant.TenantId!.Value,
            SchemaName = tenant.SchemaName!,
            DeviceName = device.Name,
            IsActive = true,
            AttLogStamp = "0",
            OperLogStamp = "0",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await uow.BiometricDeviceRegistries.AddAsync(registry, cancellationToken);
        await uow.SaveMasterChangesAsync(cancellationToken);

        return Map(device);
    }

    public async Task<DeviceResponseDto> UpdateAsync(Guid id, UpdateDeviceDto dto, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);

        var device = await uow.BiometricDevices.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Device '{id}' not found.");

        var nameChanged = false;
        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name.Trim() != device.Name)
        {
            device.Name = dto.Name.Trim();
            nameChanged = true;
        }
        if (dto.Location is not null) device.Location = dto.Location;
        if (dto.ExamGraceMinutesBefore.HasValue) device.ExamGraceMinutesBefore = Math.Max(0, dto.ExamGraceMinutesBefore.Value);
        if (dto.ExamGraceMinutesAfter.HasValue) device.ExamGraceMinutesAfter = Math.Max(0, dto.ExamGraceMinutesAfter.Value);
        if (dto.IsActive.HasValue) device.IsActive = dto.IsActive.Value;
        device.UpdatedAt = DateTime.UtcNow;

        await uow.BiometricDevices.UpdateAsync(device, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);

        if (nameChanged)
        {
            var registry = await uow.BiometricDeviceRegistries.GetBySerialNumberAsync(device.SerialNumber, cancellationToken);
            if (registry is not null)
            {
                registry.DeviceName = device.Name;
                await uow.BiometricDeviceRegistries.UpdateAsync(registry, cancellationToken);
                await uow.SaveMasterChangesAsync(cancellationToken);
            }
        }

        return Map(device);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Ready(cancellationToken);

        var device = await uow.BiometricDevices.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Device '{id}' not found.");

        await uow.BiometricDevices.DeleteAsync(device, cancellationToken);
        await uow.SaveTenantChangesAsync(cancellationToken);

        await uow.BiometricDeviceRegistries.DeleteBySerialNumberAsync(device.SerialNumber, cancellationToken);
        await uow.SaveMasterChangesAsync(cancellationToken);
    }

    private static DeviceResponseDto Map(BiometricDevice d) => new()
    {
        Id = d.Id,
        SerialNumber = d.SerialNumber,
        Name = d.Name,
        Location = d.Location,
        DeviceModel = d.DeviceModel,
        ExamGraceMinutesBefore = d.ExamGraceMinutesBefore,
        ExamGraceMinutesAfter = d.ExamGraceMinutesAfter,
        IsActive = d.IsActive,
        LastSeenAt = d.LastSeenAt,
        CreatedAt = d.CreatedAt
    };

    private async Task Ready(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenant.SchemaName) || !tenant.TenantId.HasValue)
            throw new AppException("X-Tenant-ID header is required.", 400);
        await provisioner.EnsureBiometricModuleAsync(tenant.SchemaName!, ct);
    }
}
