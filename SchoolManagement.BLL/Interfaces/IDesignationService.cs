using SchoolManagement.BLL.DTOs.Employee;
namespace SchoolManagement.BLL.Interfaces;
public interface IDesignationService { Task<IReadOnlyList<DesignationResponseDto>> GetAllAsync(CancellationToken cancellationToken = default); Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto, CancellationToken cancellationToken = default); Task<DesignationResponseDto> UpdateAsync(Guid id, UpdateDesignationDto dto, CancellationToken cancellationToken = default); Task DeleteAsync(Guid id, CancellationToken cancellationToken = default); }
