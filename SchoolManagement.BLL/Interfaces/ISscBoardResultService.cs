using SchoolManagement.BLL.DTOs.Website;

namespace SchoolManagement.BLL.Interfaces;

/// <summary>
/// Proxies Bangladesh education board SSC result lookup (eduboardresults.gov.bd),
/// matching the flow in fetch_ssc_results.py.
/// </summary>
public interface ISscBoardResultService
{
    Task<IReadOnlyList<SscBoardOptionDto>> GetBoardsAsync(CancellationToken ct = default);
    Task<SscBoardCaptchaDto> GetCaptchaAsync(bool tryAutoSolve = false, CancellationToken ct = default);
    Task<SscBoardResultDto> SearchAsync(SscBoardSearchRequestDto request, CancellationToken ct = default);
}
