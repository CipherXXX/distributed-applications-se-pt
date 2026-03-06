using System.ComponentModel.DataAnnotations;

namespace SkillForge.WebClient.Models.Api;

public class CourseDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Duration (hours)")]
    public int DurationHours { get; set; }
    public DateTime CreatedAt { get; set; }
    [Display(Name = "Active")]
    public bool IsActive { get; set; }
    public string? MaterialFileUrl { get; set; }
}
