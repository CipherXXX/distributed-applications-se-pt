using Microsoft.AspNetCore.Mvc;
using SkillForge.WebClient.Infrastructure;
using SkillForge.WebClient.Models.Api;
using SkillForge.WebClient.Services;

namespace SkillForge.WebClient.Controllers;

public class EnrollmentsController : Controller
{
    private readonly SkillForgeApiClient _api;

    public EnrollmentsController(SkillForgeApiClient api)
    {
        _api = api;
    }

    private async Task<IActionResult> EnsureAuth()
    {
        if (!_api.IsAuthenticated)
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path });
        return await Task.FromResult<IActionResult>(null!);
    }

    private IActionResult? EnsureAdmin()
    {
        if (!HttpContext.IsAdmin())
            return RedirectToAction("Index", "Home");
        return null;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, int? studentId = null, int? courseId = null, bool? completed = null, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        var isAdmin = HttpContext.IsAdmin();
        PagedResult<EnrollmentDto>? result;
        if (isAdmin)
            result = await _api.GetEnrollmentsAsync(page, pageSize, studentId, courseId, completed, cancellationToken);
        else
            result = await _api.GetMyEnrollmentsAsync(page, pageSize, cancellationToken);
        if (result == null) return RedirectToAction("Login", "Auth");
        ViewData["IsAdminEnrollments"] = isAdmin;
        return View(result);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (!HttpContext.IsAdmin())
            return RedirectToAction(nameof(Index));
        var item = await _api.GetEnrollmentAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        return View(new EnrollmentDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("StudentId,CourseId")] EnrollmentDto model, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var created = await _api.CreateEnrollmentAsync(new { model.StudentId, model.CourseId }, cancellationToken);
        if (created == null) { ModelState.AddModelError("", "Failed to create enrollment."); return View(model); }
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var item = await _api.GetEnrollmentAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProgressPercentage,Completed")] EnrollmentDto model, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var updated = await _api.UpdateEnrollmentAsync(id, new { model.ProgressPercentage, model.Completed }, cancellationToken);
        if (updated == null) { ModelState.AddModelError("", "Failed to update enrollment."); return View(model); }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var item = await _api.GetEnrollmentAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var ok = await _api.DeleteEnrollmentAsync(id, cancellationToken);
        if (!ok) return NotFound();
        return RedirectToAction(nameof(Index));
    }
}
