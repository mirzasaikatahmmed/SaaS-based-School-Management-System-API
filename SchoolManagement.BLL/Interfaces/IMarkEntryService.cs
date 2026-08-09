using SchoolManagement.BLL.DTOs.ExamMaster;

namespace SchoolManagement.BLL.Interfaces;

public interface IMarkEntryService
{
    Task<MarkEntryListResponseDto> GetListAsync(MarkEntryFilterDto filter, CancellationToken cancellationToken = default);
    Task SaveAsync(SaveMarkEntriesDto dto, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(MarkEntryFilterDto filter, CancellationToken cancellationToken = default);
}
