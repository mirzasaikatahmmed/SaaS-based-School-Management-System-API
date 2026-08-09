using SchoolManagement.BLL.DTOs.Academic;

namespace SchoolManagement.BLL.Interfaces;

public interface IStudentElectiveService
{
    Task<StudentElectiveListDto> GetClassElectivesAsync(
        Guid classId, Guid sectionId, int academicYear, string electiveGroup = "4th",
        CancellationToken cancellationToken = default);

    Task<StudentElectiveRowDto> AssignAsync(AssignStudentElectiveDto dto, CancellationToken cancellationToken = default);

    Task<StudentElectiveListDto> BulkAssignAsync(BulkAssignStudentElectiveDto dto, CancellationToken cancellationToken = default);
}
