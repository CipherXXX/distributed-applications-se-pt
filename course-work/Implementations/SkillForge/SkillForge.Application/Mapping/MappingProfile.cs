using AutoMapper;
using SkillForge.Application.DTOs;
using SkillForge.Domain.Entities;

namespace SkillForge.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Student, StudentDto>();
        CreateMap<CreateStudentDto, Student>();
        CreateMap<UpdateStudentDto, Student>().ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
        CreateMap<Course, CourseDto>();
        CreateMap<CreateCourseDto, Course>();
        CreateMap<UpdateCourseDto, Course>().ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student != null ? $"{s.Student.FirstName} {s.Student.LastName}" : null))
            .ForMember(d => d.CourseTitle, o => o.MapFrom(s => s.Course != null ? s.Course.Title : null));
        CreateMap<CreateEnrollmentDto, Enrollment>();
        CreateMap<UpdateEnrollmentDto, Enrollment>().ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
    }
}
