using AutoMapper;
using SkillForge.Application.Common;
using SkillForge.Application.DTOs;
using SkillForge.Application.Interfaces;
using SkillForge.Domain.Entities;

namespace SkillForge.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IMapper _mapper;

    public EnrollmentService(IEnrollmentRepository repository, IUserRepository userRepository, IStudentRepository studentRepository, IMapper mapper)
    {
        _repository = repository;
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _mapper = mapper;
    }

    public async Task<EnrollmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : _mapper.Map<EnrollmentDto>(entity);
    }

    public async Task<EnrollmentDto?> GetByIdForUserAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null || entity.Student?.UserId != userId)
            return null;
        return _mapper.Map<EnrollmentDto>(entity);
    }

    public async Task<PagedResult<EnrollmentDto>> GetEnrollmentsForUserAsync(int userId, int page, int pageSize, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdWithStudentsAsync(userId, cancellationToken);
        if (user == null)
            return new PagedResult<EnrollmentDto> { Items = new List<EnrollmentDto>(), TotalCount = 0, Page = page, PageSize = pageSize };
        var studentIds = user.Students?.Select(s => s.Id).ToList() ?? new List<int>();
        if (studentIds.Count == 0)
        {
            await EnsureStudentForUserAsync(user, cancellationToken);
            return new PagedResult<EnrollmentDto> { Items = new List<EnrollmentDto>(), TotalCount = 0, Page = page, PageSize = pageSize };
        }
        var items = await _repository.GetByStudentIdsAsync(studentIds, page, pageSize, sortBy, sortDesc, cancellationToken);
        var total = await _repository.GetTotalCountByStudentIdsAsync(studentIds, cancellationToken);
        return new PagedResult<EnrollmentDto>
        {
            Items = _mapper.Map<IReadOnlyList<EnrollmentDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<EnrollmentDto?> EnrollUserInCourseAsync(int userId, int courseId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdWithStudentsAsync(userId, cancellationToken);
        if (user == null) return null;
        var studentIds = user.Students?.Select(s => s.Id).ToList() ?? new List<int>();
        if (studentIds.Count == 0)
            await EnsureStudentForUserAsync(user, cancellationToken);
        user = await _userRepository.GetByIdWithStudentsAsync(userId, cancellationToken);
        var studentId = user?.Students?.FirstOrDefault()?.Id ?? 0;
        if (studentId == 0) return null;
        if (await _repository.GetByStudentAndCourseAsync(studentId, courseId, cancellationToken) != null)
            return null;
        var dto = new CreateEnrollmentDto { StudentId = studentId, CourseId = courseId };
        var created = await CreateAsync(dto, cancellationToken);
        return created;
    }

    private async Task EnsureStudentForUserAsync(Domain.Entities.User user, CancellationToken cancellationToken)
    {
        var student = new Student
        {
            FirstName = user.UserName,
            LastName = "",
            Email = user.Email,
            BirthDate = DateTime.UtcNow.AddYears(-20),
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        };
        await _studentRepository.AddAsync(student, cancellationToken);
    }

    public async Task<PagedResult<EnrollmentDto>> GetPagedAsync(int page, int pageSize, int? studentId, int? courseId, bool? completed, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetPagedAsync(page, pageSize, studentId, courseId, completed, sortBy, sortDesc, cancellationToken);
        var total = await _repository.GetTotalCountAsync(studentId, courseId, completed, cancellationToken);
        return new PagedResult<EnrollmentDto>
        {
            Items = _mapper.Map<IReadOnlyList<EnrollmentDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<EnrollmentDto> CreateAsync(CreateEnrollmentDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Enrollment>(dto);
        entity.EnrollmentDate = DateTime.UtcNow;
        entity = await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<EnrollmentDto>(entity);
    }

    public async Task<EnrollmentDto?> UpdateAsync(int id, UpdateEnrollmentDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return null;
        _mapper.Map(dto, entity);
        await _repository.UpdateAsync(entity, cancellationToken);
        return _mapper.Map<EnrollmentDto>(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        await _repository.DeleteAsync(entity, cancellationToken);
        return true;
    }
}
