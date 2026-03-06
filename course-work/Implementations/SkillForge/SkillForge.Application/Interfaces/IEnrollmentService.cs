using SkillForge.Application.Common;
using SkillForge.Application.DTOs;

namespace SkillForge.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EnrollmentDto?> GetByIdForUserAsync(int id, int userId, CancellationToken cancellationToken = default);
    Task<PagedResult<EnrollmentDto>> GetPagedAsync(int page, int pageSize, int? studentId, int? courseId, bool? completed, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default);
    Task<PagedResult<EnrollmentDto>> GetEnrollmentsForUserAsync(int userId, int page, int pageSize, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default);
    Task<EnrollmentDto?> EnrollUserInCourseAsync(int userId, int courseId, CancellationToken cancellationToken = default);
    Task<EnrollmentDto> CreateAsync(CreateEnrollmentDto dto, CancellationToken cancellationToken = default);
    Task<EnrollmentDto?> UpdateAsync(int id, UpdateEnrollmentDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
