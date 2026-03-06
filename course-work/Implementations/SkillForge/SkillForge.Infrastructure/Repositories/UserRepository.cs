using SkillForge.Application.Interfaces;
using SkillForge.Domain.Entities;
using SkillForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SkillForge.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SkillForgeDbContext _context;

    public UserRepository(SkillForgeDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdWithStudentsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Students)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
}
