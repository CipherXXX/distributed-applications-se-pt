namespace SkillForge.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}
