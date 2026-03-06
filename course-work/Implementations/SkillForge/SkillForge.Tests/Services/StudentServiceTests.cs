using AutoMapper;
using Moq;
using SkillForge.Application.DTOs;
using SkillForge.Application.Interfaces;
using SkillForge.Application.Mapping;
using SkillForge.Application.Services;
using SkillForge.Domain.Entities;
using Xunit;

namespace SkillForge.Tests.Services;

public class StudentServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IStudentRepository> _repoMock;
    private readonly Mock<IFileStorageService> _fileMock;

    public StudentServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _repoMock = new Mock<IStudentRepository>();
        _fileMock = new Mock<IFileStorageService>();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Student?)null);
        var service = new StudentService(_repoMock.Object, _fileMock.Object, _mapper);
        var result = await service.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenFound()
    {
        var entity = new Student { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@x.com", BirthDate = new DateTime(2000, 1, 1), CreatedAt = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var service = new StudentService(_repoMock.Object, _fileMock.Object, _mapper);
        var result = await service.GetByIdAsync(1);
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Jane", result.FirstName);
    }

    [Fact]
    public async Task CreateAsync_AddsEntity_AndReturnsDto()
    {
        var dto = new CreateStudentDto { FirstName = "New", LastName = "User", Email = "new@x.com", BirthDate = new DateTime(1995, 5, 5) };
        Student? captured = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .Callback<Student, CancellationToken>((s, _) => captured = s)
            .ReturnsAsync((Student s, CancellationToken _) => { s.Id = 10; return s; });
        var service = new StudentService(_repoMock.Object, _fileMock.Object, _mapper);
        var result = await service.CreateAsync(dto);
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.NotNull(captured);
        Assert.Equal("New", captured.FirstName);
    }
}
