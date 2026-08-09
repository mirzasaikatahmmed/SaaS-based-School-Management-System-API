using SchoolManagement.BLL.DTOs.Award;

namespace SchoolManagement.BLL.Interfaces;

public interface IAwardService
{
    Task<AwardListResponseDto> GetListAsync(AwardFilterDto filter, CancellationToken cancellationToken = default);
    Task<AwardListResponseDto> GetMyAwardsAsync(AwardFilterDto filter, CancellationToken cancellationToken = default);
    Task<AwardResponseDto> GiveAwardAsync(GiveAwardDto dto, CancellationToken cancellationToken = default);
    Task<AwardResponseDto> UpdateAsync(Guid id, UpdateAwardDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(AwardFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WinnerLookupDto>> GetWinnersLookupAsync(string role, CancellationToken cancellationToken = default);
}
