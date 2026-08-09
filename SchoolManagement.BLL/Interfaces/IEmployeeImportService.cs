using SchoolManagement.BLL.DTOs.Employee;
namespace SchoolManagement.BLL.Interfaces;
public interface IEmployeeImportService { Task<EmployeeImportBatchDto> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default); byte[] GetSampleCsv(); Task<IReadOnlyList<EmployeeImportBatchDto>> GetBatchesAsync(CancellationToken cancellationToken = default); Task<EmployeeImportBatchDto> GetBatchByIdAsync(Guid id, CancellationToken cancellationToken = default); }
