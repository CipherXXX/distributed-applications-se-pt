using AutoMapper;
using SkillForge.Application.Common;
using SkillForge.Application.DTOs;
using SkillForge.Application.Interfaces;
using SkillForge.Domain.Entities;

namespace SkillForge.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;
    private readonly IFileStorageService _fileStorage;
    private readonly IMapper _mapper;

    public CourseService(ICourseRepository repository, IFileStorageService fileStorage, IMapper mapper)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _mapper = mapper;
    }

    public async Task<CourseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : _mapper.Map<CourseDto>(entity);
    }

    public async Task<PagedResult<CourseDto>> GetPagedAsync(int page, int pageSize, string? title, bool? isActive, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetPagedAsync(page, pageSize, title, isActive, sortBy, sortDesc, cancellationToken);
        var total = await _repository.GetTotalCountAsync(title, isActive, cancellationToken);
        return new PagedResult<CourseDto>
        {
            Items = _mapper.Map<IReadOnlyList<CourseDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CourseDto> CreateAsync(CreateCourseDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Course>(dto);
        entity.CreatedAt = DateTime.UtcNow;
        entity = await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<CourseDto>(entity);
    }

    public async Task<CourseDto?> UpdateAsync(int id, UpdateCourseDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return null;
        _mapper.Map(dto, entity);
        await _repository.UpdateAsync(entity, cancellationToken);
        return _mapper.Map<CourseDto>(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        await _repository.DeleteAsync(entity, cancellationToken);
        return true;
    }

    public async Task<string?> UpdateMaterialAsync(int courseId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(courseId, cancellationToken);
        if (entity == null) return null;
        if (!string.IsNullOrEmpty(entity.MaterialFileUrl))
            await _fileStorage.DeleteFileAsync(entity.MaterialFileUrl, cancellationToken);
        var path = await _fileStorage.SaveFileAsync(fileStream, fileName, "materials", cancellationToken);
        entity.MaterialFileUrl = path;
        await _repository.UpdateAsync(entity, cancellationToken);
        return path;
    }
}
