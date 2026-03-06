using FluentValidation;
using SkillForge.Application.DTOs;

namespace SkillForge.Application.Validators;

public class UpdateEnrollmentDtoValidator : AbstractValidator<UpdateEnrollmentDto>
{
    public UpdateEnrollmentDtoValidator()
    {
        RuleFor(x => x.ProgressPercentage).InclusiveBetween(0, 100);
    }
}
