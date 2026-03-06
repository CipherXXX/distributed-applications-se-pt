using SkillForge.Domain.Entities;

namespace SkillForge.Application.Interfaces;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetPagedAsync(int page, int pageSize, int? studentId, int? courseId, bool? completed, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(int? studentId, int? courseId, bool? completed, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetByStudentIdsAsync(IReadOnlyList<int> studentIds, int page, int pageSize, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountByStudentIdsAsync(IReadOnlyList<int> studentIds, CancellationToken cancellationToken = default);
    Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId, CancellationToken cancellationToken = default);
    Task<Enrollment> AddAsync(Enrollment entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Enrollment entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Enrollment entity, CancellationToken cancellationToken = default);
}
