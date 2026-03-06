using SkillForge.Application.Common;
using SkillForge.Application.DTOs;

namespace SkillForge.Application.Interfaces;

public interface IStudentService
{
    Task<StudentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<StudentDto>> GetPagedAsync(int page, int pageSize, string? firstName, string? lastName, string? email, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default);
    Task<StudentDto> CreateAsync(CreateStudentDto dto, CancellationToken cancellationToken = default);
    Task<StudentDto?> UpdateAsync(int id, UpdateStudentDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<string?> UpdateProfileImageAsync(int studentId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}
