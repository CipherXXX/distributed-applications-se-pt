using SkillForge.Domain.Entities;

namespace SkillForge.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> GetPagedAsync(int page, int pageSize, string? firstName, string? lastName, string? email, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(string? firstName, string? lastName, string? email, CancellationToken cancellationToken = default);
    Task<Student> AddAsync(Student entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Student entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Student entity, CancellationToken cancellationToken = default);
}
