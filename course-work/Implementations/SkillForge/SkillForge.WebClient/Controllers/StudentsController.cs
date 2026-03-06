using Microsoft.AspNetCore.Mvc;
using SkillForge.WebClient.Infrastructure;
using SkillForge.WebClient.Models.Api;
using SkillForge.WebClient.Services;

namespace SkillForge.WebClient.Controllers;

public class StudentsController : Controller
{
    private readonly SkillForgeApiClient _api;

    public StudentsController(SkillForgeApiClient api)
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

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? firstName = null, string? lastName = null, string? email = null, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var result = await _api.GetStudentsAsync(page, pageSize, firstName, lastName, email, cancellationToken);
        if (result == null) return RedirectToAction("Login", "Auth");
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> ListJson(int page = 1, int pageSize = 10, string? firstName = null, string? lastName = null, string? email = null, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var result = await _api.GetStudentsAsync(page, pageSize, firstName, lastName, email, cancellationToken);
        if (result == null) return Unauthorized();
        return Json(result);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var item = await _api.GetStudentAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        return View(new StudentDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,BirthDate")] StudentDto model, IFormFile? profileImage, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var created = await _api.CreateStudentAsync(new { model.FirstName, model.LastName, model.Email, model.BirthDate }, cancellationToken);
        if (created == null) { ModelState.AddModelError("", "Failed to create student."); return View(model); }
        if (profileImage != null && profileImage.Length > 0)
        {
            var uploadUrl = await _api.UploadProfileImageAsync(created.Id, profileImage, cancellationToken);
            if (uploadUrl == null)
                TempData["Warning"] = "Student created but profile image upload failed (check file type and size). You can add an image later via Edit.";
        }
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var item = await _api.GetStudentAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("FirstName,LastName,Email,BirthDate")] StudentDto model, IFormFile? profileImage, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var updated = await _api.UpdateStudentAsync(id, new { model.FirstName, model.LastName, model.Email, model.BirthDate }, cancellationToken);
        if (updated == null) { ModelState.AddModelError("", "Failed to update student."); return View(model); }
        if (profileImage != null && profileImage.Length > 0)
        {
            await _api.UploadProfileImageAsync(id, profileImage, cancellationToken);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var auth = await EnsureAuth();
        if (auth != null) return auth;
        if (EnsureAdmin() is IActionResult redirect) return redirect;
        var item = await _api.GetStudentAsync(id, cancellationToken);
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
        var ok = await _api.DeleteStudentAsync(id, cancellationToken);
        if (!ok) return NotFound();
        return RedirectToAction(nameof(Index));
    }
}
