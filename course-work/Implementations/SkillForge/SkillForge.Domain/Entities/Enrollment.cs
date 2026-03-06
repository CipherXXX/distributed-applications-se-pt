namespace SkillForge.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public decimal ProgressPercentage { get; set; }
    public bool Completed { get; set; }
    public string? CertificateFileUrl { get; set; }
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
