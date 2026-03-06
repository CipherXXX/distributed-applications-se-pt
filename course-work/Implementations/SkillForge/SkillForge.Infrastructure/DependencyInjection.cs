using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkillForge.Application.Interfaces;
using SkillForge.Infrastructure.Data;
using SkillForge.Infrastructure.Repositories;
using SkillForge.Infrastructure.Services;

namespace SkillForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SkillForgeDbContext>(o => o.UseSqlServer(connectionString));
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        return services;
    }
}
