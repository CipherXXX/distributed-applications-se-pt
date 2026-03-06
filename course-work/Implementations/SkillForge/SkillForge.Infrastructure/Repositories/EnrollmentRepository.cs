using Microsoft.EntityFrameworkCore;
using SkillForge.Application.Interfaces;
using SkillForge.Domain.Entities;
using SkillForge.Infrastructure.Data;

namespace SkillForge.Infrastructure.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly SkillForgeDbContext _context;

    public EnrollmentRepository(SkillForgeDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Enrollment>> GetPagedAsync(int page, int pageSize, int? studentId, int? courseId, bool? completed, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default)
    {
        var query = _context.Enrollments.Include(e => e.Student).Include(e => e.Course).AsQueryable();
        if (studentId.HasValue)
            query = query.Where(e => e.StudentId == studentId.Value);
        if (courseId.HasValue)
            query = query.Where(e => e.CourseId == courseId.Value);
        if (completed.HasValue)
            query = query.Where(e => e.Completed == completed.Value);
        query = (sortBy?.ToLowerInvariant()) switch
        {
            "enrollmentdate" => sortDesc ? query.OrderByDescending(e => e.EnrollmentDate) : query.OrderBy(e => e.EnrollmentDate),
            "progresspercentage" => sortDesc ? query.OrderByDescending(e => e.ProgressPercentage) : query.OrderBy(e => e.ProgressPercentage),
            "completed" => sortDesc ? query.OrderByDescending(e => e.Completed) : query.OrderBy(e => e.Completed),
            _ => sortDesc ? query.OrderByDescending(e => e.EnrollmentDate) : query.OrderBy(e => e.EnrollmentDate)
        };
        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(int? studentId, int? courseId, bool? completed, CancellationToken cancellationToken = default)
    {
        var query = _context.Enrollments.AsQueryable();
        if (studentId.HasValue)
            query = query.Where(e => e.StudentId == studentId.Value);
        if (courseId.HasValue)
            query = query.Where(e => e.CourseId == courseId.Value);
        if (completed.HasValue)
            query = query.Where(e => e.Completed == completed.Value);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Enrollment>> GetByStudentIdsAsync(IReadOnlyList<int> studentIds, int page, int pageSize, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default)
    {
        if (studentIds.Count == 0)
            return new List<Enrollment>();
        var query = _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => studentIds.Contains(e.StudentId));
        query = (sortBy?.ToLowerInvariant()) switch
        {
            "enrollmentdate" => sortDesc ? query.OrderByDescending(e => e.EnrollmentDate) : query.OrderBy(e => e.EnrollmentDate),
            "progresspercentage" => sortDesc ? query.OrderByDescending(e => e.ProgressPercentage) : query.OrderBy(e => e.ProgressPercentage),
            "completed" => sortDesc ? query.OrderByDescending(e => e.Completed) : query.OrderBy(e => e.Completed),
            _ => sortDesc ? query.OrderByDescending(e => e.EnrollmentDate) : query.OrderBy(e => e.EnrollmentDate)
        };
        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountByStudentIdsAsync(IReadOnlyList<int> studentIds, CancellationToken cancellationToken = default)
    {
        if (studentIds.Count == 0)
            return 0;
        return await _context.Enrollments
            .Where(e => studentIds.Contains(e.StudentId))
            .CountAsync(cancellationToken);
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId, cancellationToken);
    }

    public async Task<Enrollment> AddAsync(Enrollment entity, CancellationToken cancellationToken = default)
    {
        _context.Enrollments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Enrollment entity, CancellationToken cancellationToken = default)
    {
        _context.Enrollments.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Enrollment entity, CancellationToken cancellationToken = default)
    {
        _context.Enrollments.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
