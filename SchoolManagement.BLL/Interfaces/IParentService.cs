using SchoolManagement.BLL.DTOs.Parents;

namespace SchoolManagement.BLL.Interfaces;

public interface IParentService
{
    Task<ParentListResponseDto> GetListAsync(ParentListFilterDto filter, CancellationToken cancellationToken = default);
    Task<ParentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParentDetailDto> GetMeAsync(CancellationToken cancellationToken = default);
    Task<ParentDetailDto> CreateAsync(AddParentDto dto, CancellationToken cancellationToken = default);
    Task<ParentDetailDto> UpdateAsync(Guid id, UpdateParentDto dto, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParentDetailDto> UploadPhotoAsync(Guid id, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(ParentListFilterDto filter, CancellationToken cancellationToken = default);
}
