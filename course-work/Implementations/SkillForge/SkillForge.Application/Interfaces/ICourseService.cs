using SkillForge.Application.Common;
using SkillForge.Application.DTOs;

namespace SkillForge.Application.Interfaces;

public interface ICourseService
{
    Task<CourseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<CourseDto>> GetPagedAsync(int page, int pageSize, string? title, bool? isActive, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default);
    Task<CourseDto> CreateAsync(CreateCourseDto dto, CancellationToken cancellationToken = default);
    Task<CourseDto?> UpdateAsync(int id, UpdateCourseDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<string?> UpdateMaterialAsync(int courseId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}
