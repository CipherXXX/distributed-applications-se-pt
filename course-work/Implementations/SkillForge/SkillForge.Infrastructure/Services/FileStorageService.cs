using Microsoft.Extensions.Options;
using SkillForge.Application.Interfaces;

namespace SkillForge.Infrastructure.Services;

public class FileStorageOptions
{
    public string BasePath { get; set; } = "uploads";
}

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService(IOptions<FileStorageOptions> options)
    {
        _basePath = Path.GetFullPath(options.Value.BasePath);
    }

    public async Task<string> SaveFileAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(dir);
        var safeName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(dir, safeName);
        await using var fs = File.Create(fullPath);
        await stream.CopyToAsync(fs, cancellationToken);
        return Path.Combine(folder, safeName).Replace("\\", "/");
    }

    public Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath.Replace("/", "\\"));
        if (!File.Exists(fullPath)) return Task.FromResult(false);
        try
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
