using SchoolManagement.DAL.Entities.Master;

namespace SchoolManagement.DAL.Repositories.Interfaces;

public class SchoolSearchFilter
{
    public string? Search { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? SchoolType { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public interface ISchoolRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Tenant> Items, int TotalCount)> SearchAsync(SchoolSearchFilter filter, CancellationToken cancellationToken = default);
    Task<Tenant> AddAsync(Tenant school, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant school, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
