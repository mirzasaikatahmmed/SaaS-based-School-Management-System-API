using SchoolManagement.BLL.DTOs.Reports;

namespace SchoolManagement.BLL.Interfaces;

public interface IExaminationReportService
{
    Task<ExamReportStudentListDto> GetStudentsAsync(ExamReportStudentFilterDto filter, CancellationToken cancellationToken = default);
    Task<ReportCardBatchDto> GenerateReportCardsAsync(GenerateExamCardsRequestDto request, CancellationToken cancellationToken = default);
    Task<ReportCardBatchDto> GenerateProgressReportsAsync(GenerateExamCardsRequestDto request, CancellationToken cancellationToken = default);
    Task<TabulationSheetDto> GetTabulationSheetAsync(
        Guid examId, Guid classId, Guid sectionId, int academicYear, CancellationToken cancellationToken = default);
}
