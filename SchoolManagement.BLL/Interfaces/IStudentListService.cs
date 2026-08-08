using SchoolManagement.BLL.DTOs.Student;
using StudentListPageDto = SchoolManagement.BLL.DTOs.StudentList.StudentListResponseDto;
using SchoolManagement.BLL.DTOs.StudentList;

namespace SchoolManagement.BLL.Interfaces;

public interface IStudentListService
{
    Task<StudentListPageDto> GetListAsync(StudentListFilterDto filter, CancellationToken cancellationToken = default);
    Task<StudentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StudentDetailDto> GetMeAsync(CancellationToken cancellationToken = default);
    Task<StudentDetailDto> UpdateAsync(Guid id, UpdateAdmissionDto dto, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BulkDeleteResultDto> BulkDeleteAsync(BulkDeleteDto dto, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(StudentListFilterDto filter, CancellationToken cancellationToken = default);

    Task<StudentListPageDto> GetLoginDeactivateListAsync(StudentListFilterDto filter, CancellationToken cancellationToken = default);
    Task ToggleLoginAsync(Guid id, LoginDeactivateDto dto, CancellationToken cancellationToken = default);

    Task<StudentListPageDto> GetDeactivateReasonsAsync(StudentListFilterDto filter, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, DeactivateReasonDto dto, CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
}
