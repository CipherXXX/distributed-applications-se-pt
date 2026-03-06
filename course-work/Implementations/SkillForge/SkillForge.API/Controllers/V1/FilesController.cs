using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SkillForge.Infrastructure.Services;

namespace SkillForge.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly string _uploadsPath;

    public FilesController(IOptions<FileStorageOptions> options)
    {
        _uploadsPath = Path.GetFullPath(options.Value.BasePath);
    }

    /// <summary>
    /// Download a file from uploads (profiles or materials). Requires authentication.
    /// </summary>
    [HttpGet("download")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Download([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest(new { error = "Path is required." });
        path = path.Replace("/", Path.DirectorySeparatorChar.ToString()).TrimStart(Path.DirectorySeparatorChar);
        if (path.Contains("..", StringComparison.Ordinal))
            return BadRequest(new { error = "Invalid path." });
        var fullPath = Path.GetFullPath(Path.Combine(_uploadsPath, path));
        if (!fullPath.StartsWith(_uploadsPath, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Invalid path." });
        if (!System.IO.File.Exists(fullPath))
            return NotFound();
        var contentType = GetContentType(Path.GetExtension(fullPath));
        var fileName = Path.GetFileName(fullPath);
        return PhysicalFile(fullPath, contentType, fileName);
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}
