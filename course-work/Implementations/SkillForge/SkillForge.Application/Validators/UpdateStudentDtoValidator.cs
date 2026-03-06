using FluentValidation;
using SkillForge.Application.DTOs;

namespace SkillForge.Application.Validators;

public class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentDto>
{
    public UpdateStudentDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.BirthDate).NotEmpty().LessThan(DateTime.UtcNow.AddYears(-10));
    }
}
