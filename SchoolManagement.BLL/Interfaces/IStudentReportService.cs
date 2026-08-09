using SchoolManagement.BLL.DTOs.Reports;

namespace SchoolManagement.BLL.Interfaces;

public interface IStudentReportService
{
    Task<LoginCredentialReportDto> GetLoginCredentialsAsync(StudentReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ResetStudentPasswordResultDto> ResetPasswordAsync(Guid studentId, ResetStudentPasswordDto dto, CancellationToken cancellationToken = default);
    Task<AdmissionReportDto> GetAdmissionReportAsync(StudentReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClassSectionReportRowDto>> GetClassSectionReportAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SiblingReportRowDto>> GetSiblingReportAsync(StudentReportFilterDto filter, CancellationToken cancellationToken = default);
}
