using SkillForge.Application.DTOs;

namespace SkillForge.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse?> RegisterAdminAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
