using Microsoft.AspNetCore.Mvc;
using SkillForge.WebClient.Infrastructure;
using SkillForge.WebClient.Models.Api;
using SkillForge.WebClient.Services;

namespace SkillForge.WebClient.Controllers;

public class AuthController : Controller
{
    private readonly SkillForgeApiClient _api;

    public AuthController(SkillForgeApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);
        var result = await _api.LoginAsync(new { model.UserName, model.Password }, cancellationToken);
        if (result == null)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }
        HttpContext.Session.SetString(AuthHelper.SessionUserNameKey, result.UserName);
        HttpContext.Session.SetString(AuthHelper.SessionIsAdminKey, result.IsAdmin ? "True" : "False");
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _api.RegisterAsync(new { model.UserName, model.Email, model.Password, model.FirstName, model.LastName, model.BirthDate }, cancellationToken);
        if (result == null)
        {
            ModelState.AddModelError("", "Username or email already exists.");
            return View(model);
        }
        HttpContext.Session.SetString(AuthHelper.SessionUserNameKey, result.UserName);
        HttpContext.Session.SetString(AuthHelper.SessionIsAdminKey, result.IsAdmin ? "True" : "False");
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        _api.ClearAuthToken();
        HttpContext.Session.Remove(AuthHelper.SessionUserNameKey);
        HttpContext.Session.Remove(AuthHelper.SessionIsAdminKey);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> AddAdmin(CancellationToken cancellationToken = default)
    {
        if (!_api.IsAuthenticated)
            return RedirectToAction("Login", new { returnUrl = "/Auth/AddAdmin" });
        if (!HttpContext.IsAdmin())
            return RedirectToAction("Index", "Home");
        return View(new RegisterDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAdmin(RegisterDto model, CancellationToken cancellationToken = default)
    {
        if (!_api.IsAuthenticated)
            return RedirectToAction("Login");
        if (!HttpContext.IsAdmin())
            return RedirectToAction("Index", "Home");
        if (!ModelState.IsValid) return View(model);
        var result = await _api.RegisterAdminAsync(new { model.UserName, model.Email, model.Password, model.FirstName, model.LastName, model.BirthDate }, cancellationToken);
        if (result == null)
        {
            ModelState.AddModelError("", "Username or email already exists.");
            return View(model);
        }
        TempData["Success"] = $"Admin user '{result.UserName}' has been created. They can log in with the provided credentials.";
        return RedirectToAction(nameof(AddAdmin));
    }
}
