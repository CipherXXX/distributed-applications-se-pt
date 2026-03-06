using Microsoft.EntityFrameworkCore;
using SkillForge.Application.Interfaces;
using SkillForge.Domain.Entities;
using SkillForge.Infrastructure.Data;

namespace SkillForge.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly SkillForgeDbContext _context;

    public StudentRepository(SkillForgeDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Students.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetPagedAsync(int page, int pageSize, string? firstName, string? lastName, string? email, string? sortBy, bool sortDesc, CancellationToken cancellationToken = default)
    {
        var query = _context.Students.AsQueryable();
        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(s => s.FirstName.Contains(firstName));
        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(s => s.LastName.Contains(lastName));
        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(s => s.Email.Contains(email));
        query = (sortBy?.ToLowerInvariant()) switch
        {
            "lastname" => sortDesc ? query.OrderByDescending(s => s.LastName) : query.OrderBy(s => s.LastName),
            "email" => sortDesc ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email),
            "birthdate" => sortDesc ? query.OrderByDescending(s => s.BirthDate) : query.OrderBy(s => s.BirthDate),
            "createdat" => sortDesc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(s => s.FirstName) : query.OrderBy(s => s.FirstName)
        };
        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(string? firstName, string? lastName, string? email, CancellationToken cancellationToken = default)
    {
        var query = _context.Students.AsQueryable();
        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(s => s.FirstName.Contains(firstName));
        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(s => s.LastName.Contains(lastName));
        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(s => s.Email.Contains(email));
        return await query.CountAsync(cancellationToken);
    }

    public async Task<Student> AddAsync(Student entity, CancellationToken cancellationToken = default)
    {
        _context.Students.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Student entity, CancellationToken cancellationToken = default)
    {
        _context.Students.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Student entity, CancellationToken cancellationToken = default)
    {
        _context.Students.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
