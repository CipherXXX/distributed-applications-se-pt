using System.ComponentModel.DataAnnotations;

namespace SkillForge.WebClient.Models.Api;

public class EnrollmentDto
{
    public int Id { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    [Display(Name = "Student ID")]
    public int StudentId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    [Display(Name = "Course ID")]
    public int CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; }

    [Range(0, 100)]
    [Display(Name = "Progress %")]
    public decimal ProgressPercentage { get; set; }
    public bool Completed { get; set; }
    public string? CertificateFileUrl { get; set; }
    public string? StudentName { get; set; }
    public string? CourseTitle { get; set; }
}
