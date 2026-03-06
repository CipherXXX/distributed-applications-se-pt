using Microsoft.EntityFrameworkCore;
using SkillForge.Application.Interfaces;
using SkillForge.Domain.Entities;
using SkillForge.Infrastructure.Data;

namespace SkillForge.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly SkillForgeDbContext _context;

    public CourseRepository(SkillForgeDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Courses.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<Course>> GetPagedAsync(int page, int pageSize, string? title, bool? isActive, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default)
    {
        var query = _context.Courses.AsQueryable();
        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(c => c.Title.Contains(title));
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);
        query = (sortBy?.ToLowerInvariant()) switch
        {
            "title" => sortDesc ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title),
            "price" => sortDesc ? query.OrderByDescending(c => c.Price) : query.OrderBy(c => c.Price),
            "durationhours" => sortDesc ? query.OrderByDescending(c => c.DurationHours) : query.OrderBy(c => c.DurationHours),
            "createdat" => sortDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title)
        };
        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(string? title, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = _context.Courses.AsQueryable();
        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(c => c.Title.Contains(title));
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<Course> AddAsync(Course entity, CancellationToken cancellationToken = default)
    {
        _context.Courses.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Course entity, CancellationToken cancellationToken = default)
    {
        _context.Courses.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Course entity, CancellationToken cancellationToken = default)
    {
        _context.Courses.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
