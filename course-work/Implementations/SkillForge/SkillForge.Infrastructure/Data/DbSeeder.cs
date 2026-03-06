using Microsoft.EntityFrameworkCore;
using SkillForge.Domain.Entities;

namespace SkillForge.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(SkillForgeDbContext context)
    {
        await context.Database.MigrateAsync();
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
        if (adminUser != null)
        {
            adminUser.IsAdmin = true;
            await context.SaveChangesAsync();
        }
        if (await context.Students.AnyAsync())
            return;
        var user = new User
        {
            UserName = "admin",
            Email = "admin@skillforge.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var students = new List<Student>
        {
            new() { FirstName = "Ivan", LastName = "Petrov", Email = "ivan@example.com", BirthDate = new DateTime(2000, 5, 15), CreatedAt = DateTime.UtcNow },
            new() { FirstName = "Maria", LastName = "Ivanova", Email = "maria@example.com", BirthDate = new DateTime(1999, 8, 22), CreatedAt = DateTime.UtcNow },
            new() { FirstName = "Alex", LastName = "Sidorov", Email = "alex@example.com", BirthDate = new DateTime(2001, 1, 10), CreatedAt = DateTime.UtcNow }
        };
        context.Students.AddRange(students);
        await context.SaveChangesAsync();
        var courses = new List<Course>
        {
            new() { Title = "C# Programming", Description = "Learn C# and .NET", Price = 99.99m, DurationHours = 40, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Title = "Web API Development", Description = "RESTful APIs with ASP.NET Core", Price = 129.99m, DurationHours = 30, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Title = "Entity Framework Core", Description = "Data access with EF Core", Price = 79.99m, DurationHours = 20, IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();
        var enrollments = new List<Enrollment>
        {
            new() { StudentId = 1, CourseId = 1, EnrollmentDate = DateTime.UtcNow.AddDays(-30), ProgressPercentage = 75, Completed = false },
            new() { StudentId = 1, CourseId = 2, EnrollmentDate = DateTime.UtcNow.AddDays(-10), ProgressPercentage = 20, Completed = false },
            new() { StudentId = 2, CourseId = 1, EnrollmentDate = DateTime.UtcNow.AddDays(-60), ProgressPercentage = 100, Completed = true },
            new() { StudentId = 3, CourseId = 3, EnrollmentDate = DateTime.UtcNow.AddDays(-5), ProgressPercentage = 10, Completed = false }
        };
        context.Enrollments.AddRange(enrollments);
        await context.SaveChangesAsync();
    }
}
