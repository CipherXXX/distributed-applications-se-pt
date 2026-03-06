using FluentValidation.TestHelper;
using SkillForge.Application.DTOs;
using SkillForge.Application.Validators;
using Xunit;

namespace SkillForge.Tests.Validators;

public class CreateStudentDtoValidatorTests
{
    private readonly CreateStudentDtoValidator _validator = new();

    [Fact]
    public void Should_HaveError_When_FirstNameIsEmpty()
    {
        var model = new CreateStudentDto { FirstName = "", LastName = "Test", Email = "a@b.com", BirthDate = DateTime.UtcNow.AddYears(-20) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_NotHaveError_When_Valid()
    {
        var model = new CreateStudentDto { FirstName = "John", LastName = "Doe", Email = "john@example.com", BirthDate = DateTime.UtcNow.AddYears(-20) };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_EmailInvalid()
    {
        var model = new CreateStudentDto { FirstName = "John", LastName = "Doe", Email = "invalid", BirthDate = DateTime.UtcNow.AddYears(-20) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
