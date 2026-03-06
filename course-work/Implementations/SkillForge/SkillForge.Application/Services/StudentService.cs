using AutoMapper;
using SkillForge.Application.Common;
using SkillForge.Application.DTOs;
using SkillForge.Application.Interfaces;
using SkillForge.Domain.Entities;

namespace SkillForge.Application.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;
    private readonly IFileStorageService _fileStorage;
    private readonly IMapper _mapper;

    public StudentService(IStudentRepository repository, IFileStorageService fileStorage, IMapper mapper)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _mapper = mapper;
    }

    public async Task<StudentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : _mapper.Map<StudentDto>(entity);
    }

    public async Task<PagedResult<StudentDto>> GetPagedAsync(int page, int pageSize, string? firstName, string? lastName, string? email, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetPagedAsync(page, pageSize, firstName, lastName, email, sortBy, sortDesc, cancellationToken);
        var total = await _repository.GetTotalCountAsync(firstName, lastName, email, cancellationToken);
        return new PagedResult<StudentDto>
        {
            Items = _mapper.Map<IReadOnlyList<StudentDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<StudentDto> CreateAsync(CreateStudentDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Student>(dto);
        entity.CreatedAt = DateTime.UtcNow;
        entity = await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<StudentDto>(entity);
    }

    public async Task<StudentDto?> UpdateAsync(int id, UpdateStudentDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return null;
        _mapper.Map(dto, entity);
        await _repository.UpdateAsync(entity, cancellationToken);
        return _mapper.Map<StudentDto>(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        await _repository.DeleteAsync(entity, cancellationToken);
        return true;
    }

    public async Task<string?> UpdateProfileImageAsync(int studentId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(studentId, cancellationToken);
        if (entity == null) return null;
        if (!string.IsNullOrEmpty(entity.ProfileImageUrl))
            await _fileStorage.DeleteFileAsync(entity.ProfileImageUrl, cancellationToken);
        var path = await _fileStorage.SaveFileAsync(fileStream, fileName, "profiles", cancellationToken);
        entity.ProfileImageUrl = path;
        await _repository.UpdateAsync(entity, cancellationToken);
        return path;
    }
}
