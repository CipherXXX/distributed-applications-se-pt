using FluentValidation;
using SkillForge.Application.DTOs;

namespace SkillForge.Application.Validators;

public class CreateEnrollmentDtoValidator : AbstractValidator<CreateEnrollmentDto>
{
    public CreateEnrollmentDtoValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.CourseId).GreaterThan(0);
    }
}
