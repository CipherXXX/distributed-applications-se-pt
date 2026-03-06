using SkillForge.Domain.Entities;

namespace SkillForge.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithStudentsAsync(int id, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User entity, CancellationToken cancellationToken = default);
}
