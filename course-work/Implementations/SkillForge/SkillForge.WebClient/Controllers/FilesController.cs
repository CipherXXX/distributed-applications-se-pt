using Microsoft.AspNetCore.Mvc;
using SkillForge.WebClient.Services;

namespace SkillForge.WebClient.Controllers;

/// <summary>
/// Proxies file download from the API so that only authenticated members can access uploaded files.
/// </summary>
public class FilesController : Controller
{
    private readonly SkillForgeApiClient _api;

    public FilesController(SkillForgeApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Download a file (profile image or course material). Requires login (any member or admin).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Download([FromQuery] string path, CancellationToken cancellationToken = default)
    {
        if (!_api.IsAuthenticated)
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest();
        var response = await _api.GetFileAsync(path, cancellationToken);
        if (response == null || !response.IsSuccessStatusCode)
            return NotFound();
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? Path.GetFileName(path);
        return File(stream, contentType, fileName);
    }
}
