using SkillForge.Domain.Entities;

namespace SkillForge.Application.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Course>> GetPagedAsync(int page, int pageSize, string? title, bool? isActive, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(string? title, bool? isActive, CancellationToken cancellationToken = default);
    Task<Course> AddAsync(Course entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Course entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Course entity, CancellationToken cancellationToken = default);
}
