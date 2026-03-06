using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using SkillForge.Application.DTOs;
using SkillForge.Application.Interfaces;
using SkillForge.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace SkillForge.Infrastructure.Services;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SkillForge";
    public string Audience { get; set; } = "SkillForge";
    public int ExpirationMinutes { get; set; } = 60;
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUserRepository userRepository, IStudentRepository studentRepository, IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUserNameAsync(request.UserName, cancellationToken);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;
        return BuildResponse(user);
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.GetByUserNameAsync(request.UserName, cancellationToken) != null)
            return null;
        if (await _userRepository.GetByEmailAsync(request.Email, cancellationToken) != null)
            return null;
        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };
        user = await _userRepository.AddAsync(user, cancellationToken);
        var student = new Student
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            BirthDate = request.BirthDate.Kind == DateTimeKind.Utc ? request.BirthDate : DateTime.SpecifyKind(request.BirthDate, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        };
        await _studentRepository.AddAsync(student, cancellationToken);
        return BuildResponse(user);
    }

    public async Task<AuthResponse?> RegisterAdminAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.GetByUserNameAsync(request.UserName, cancellationToken) != null)
            return null;
        if (await _userRepository.GetByEmailAsync(request.Email, cancellationToken) != null)
            return null;
        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };
        user = await _userRepository.AddAsync(user, cancellationToken);
        var student = new Student
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            BirthDate = request.BirthDate.Kind == DateTimeKind.Utc ? request.BirthDate : DateTime.SpecifyKind(request.BirthDate, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        };
        await _studentRepository.AddAsync(student, cancellationToken);
        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
        var token = GenerateToken(user, expires);
        return new AuthResponse
        {
            Token = token,
            UserName = user.UserName,
            Email = user.Email,
            IsAdmin = user.IsAdmin,
            ExpiresAt = expires
        };
    }

    private string GenerateToken(User user, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };
        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: expires,
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
