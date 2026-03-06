using Microsoft.EntityFrameworkCore;
using SkillForge.Domain.Entities;

namespace SkillForge.Infrastructure.Data;

public class SkillForgeDbContext : DbContext
{
    public SkillForgeDbContext(DbContextOptions<SkillForgeDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.ProfileImageUrl).HasMaxLength(500);
            e.HasOne(x => x.User).WithMany(u => u.Students).HasForeignKey(x => x.UserId).IsRequired(false);
        });
        modelBuilder.Entity<Course>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.MaterialFileUrl).HasMaxLength(500);
        });
        modelBuilder.Entity<Enrollment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ProgressPercentage).HasPrecision(5, 2);
            e.Property(x => x.CertificateFileUrl).HasMaxLength(500);
            e.HasOne(x => x.Student).WithMany(s => s.Enrollments).HasForeignKey(x => x.StudentId);
            e.HasOne(x => x.Course).WithMany(c => c.Enrollments).HasForeignKey(x => x.CourseId);
        });
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.IsAdmin).HasDefaultValue(false);
        });
    }
}
