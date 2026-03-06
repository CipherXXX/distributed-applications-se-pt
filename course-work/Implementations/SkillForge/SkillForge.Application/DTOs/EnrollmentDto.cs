namespace SkillForge.Application.DTOs;

public class EnrollmentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public decimal ProgressPercentage { get; set; }
    public bool Completed { get; set; }
    public string? CertificateFileUrl { get; set; }
    public string? StudentName { get; set; }
    public string? CourseTitle { get; set; }
}

public class CreateEnrollmentDto
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
}

public class UpdateEnrollmentDto
{
    public decimal ProgressPercentage { get; set; }
    public bool Completed { get; set; }
}

public class EnrollMeRequest
{
    public int CourseId { get; set; }
}
