using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public interface IAdmissionLookupRepository
{
    Task<IReadOnlyList<ClassEntity>> GetClassesAsync(CancellationToken cancellationToken = default);
    Task<ClassEntity?> GetClassByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Section>> GetSectionsByClassIdAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<Section?> GetSectionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<StudentCategory?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TransportRoute>> GetTransportRoutesAsync(CancellationToken cancellationToken = default);
    Task<TransportRoute?> GetTransportRouteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Hostel>> GetHostelsAsync(CancellationToken cancellationToken = default);
    Task<Hostel?> GetHostelByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HostelRoom>> GetHostelRoomsAsync(Guid hostelId, CancellationToken cancellationToken = default);
    Task<HostelRoom?> GetHostelRoomByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
