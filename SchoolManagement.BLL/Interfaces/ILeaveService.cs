using SchoolManagement.BLL.DTOs.AdvanceSalary;
using SchoolManagement.BLL.DTOs.Leave;

namespace SchoolManagement.BLL.Interfaces;

public interface ILeaveService
{
    Task<LeaveListResponseDto> GetMyListAsync(LeaveFilterDto filter, CancellationToken cancellationToken = default);
    Task<LeaveListItemDto> CreateMyAsync(CreateLeaveRequestDto dto, CancellationToken cancellationToken = default);
    Task CancelMyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LeaveListResponseDto> GetManageListAsync(LeaveManageFilterDto filter, CancellationToken cancellationToken = default);
    Task<LeaveListItemDto> AdminCreateAsync(AdminCreateLeaveRequestDto dto, CancellationToken cancellationToken = default);
    Task<LeaveListItemDto> ApproveAsync(Guid id, ReviewLeaveDto dto, CancellationToken cancellationToken = default);
    Task<LeaveListItemDto> RejectAsync(Guid id, ReviewLeaveDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LeaveListItemDto> UploadAttachmentAsync(Guid id, Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(LeaveManageFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveCategoryLookupDto>> GetMyLeaveTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HrEmployeeLookupDto>> GetEmployeeLookupAsync(string role, CancellationToken cancellationToken = default);
}
