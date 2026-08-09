using SchoolManagement.BLL.DTOs.Employee;
namespace SchoolManagement.BLL.Interfaces;
public interface IDepartmentService { Task<IReadOnlyList<DepartmentResponseDto>> GetAllAsync(CancellationToken cancellationToken = default); Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default); Task<DepartmentResponseDto> UpdateAsync(Guid id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default); Task DeleteAsync(Guid id, CancellationToken cancellationToken = default); }
