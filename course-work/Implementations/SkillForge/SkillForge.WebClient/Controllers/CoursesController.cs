using Microsoft.AspNetCore.Mvc;
using SkillForge.WebClient.Infrastructure;
using SkillForge.WebClient.Models.Api;
using SkillForge.WebClient.Services;

namespace SkillForge.WebClient.Controllers;

public class CoursesController : Controller
{
    private readonly SkillForgeApiClient _api;

    public CoursesController(SkillForgeApiClient api)
    {
        _api = api;
    }

    private async Task<IActionResult> EnsureAuth()
    {
        if (!_api.IsAuthenticated)
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path });
        return await Task.FromResult<IActionResult>(null!);
    }

    private IActionResult EnsureAdmin()
    {
        if (!HttpContext.IsAdmin())
            return RedirectToAction(nameof(Index));
        return null!;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? title = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        var result = await _api.GetCoursesAsync(page, pageSize, title, isActive, cancellationToken);
        if (result == null) return RedirectToAction("Login", "Auth");
        return View(result);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        var item = await _api.GetCourseAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (HttpContext.IsAdmin())
            return RedirectToAction(nameof(Details), new { id });
        var enrolled = await _api.EnrollMeInCourseAsync(id, cancellationToken);
        if (enrolled != null)
            TempData["EnrollSuccess"] = "You have been enrolled in this course.";
        else
            TempData["EnrollError"] = "Could not enroll. You may already be enrolled in this course.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult adminRedirect) return adminRedirect;
        return View(new CourseDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,Price,DurationHours,IsActive")] CourseDto model, IFormFile? materialFile, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult adminRedirect) return adminRedirect;
        var created = await _api.CreateCourseAsync(new { model.Title, model.Description, model.Price, model.DurationHours, model.IsActive }, cancellationToken);
        if (created == null) { ModelState.AddModelError("", "Failed to create course."); return View(model); }
        if (materialFile != null && materialFile.Length > 0)
        {
            var uploadUrl = await _api.UploadCourseMaterialAsync(created.Id, materialFile, cancellationToken);
            if (uploadUrl == null)
                TempData["Warning"] = "Course created but material file upload failed (check file type and size). You can add a file later via Edit.";
        }
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult adminRedirect) return adminRedirect;
        var item = await _api.GetCourseAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Title,Description,Price,DurationHours,IsActive")] CourseDto model, IFormFile? materialFile, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult adminRedirect) return adminRedirect;
        var updated = await _api.UpdateCourseAsync(id, new { model.Title, model.Description, model.Price, model.DurationHours, model.IsActive }, cancellationToken);
        if (updated == null) { ModelState.AddModelError("", "Failed to update course."); return View(model); }
        if (materialFile != null && materialFile.Length > 0)
        {
            await _api.UploadCourseMaterialAsync(id, materialFile, cancellationToken);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult adminRedirect) return adminRedirect;
        var item = await _api.GetCourseAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult adminRedirect) return adminRedirect;
        var ok = await _api.DeleteCourseAsync(id, cancellationToken);
        if (!ok) return NotFound();
        return RedirectToAction(nameof(Index));
    }
}
