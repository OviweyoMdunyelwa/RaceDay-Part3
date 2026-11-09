using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class AuthController : Controller
{
    private readonly RaceDayApiClient _api;

    public AuthController(RaceDayApiClient api)
    {
        _api = api;
    }

    // ---------- REGISTER ----------
    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var request = new RegisterRequest
        {
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            Email = vm.Email,
            Password = vm.Password,
            Role = vm.Role,
            PhoneNumber = vm.PhoneNumber
        };

        HttpResponseMessage response;
        try
        {
            response = await _api.SendAsync(HttpMethod.Post, "api/auth/register", request);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "RaceDay could not reach the server. Please try again.");
            return View(vm);
        }

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await GetFriendlyError(response, "Registration failed."));
            return View(vm);
        }

        TempData["Success"] = "Registration successful. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    // ---------- LOGIN ----------
    [HttpGet]
    public IActionResult Login() => View(new LoginVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        HttpResponseMessage response;
        try
        {
            response = await _api.SendAsync(HttpMethod.Post, "api/auth/login",
                new LoginRequest { Email = vm.Email, Password = vm.Password });
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "RaceDay could not reach the server. Please try again.");
            return View(vm);
        }

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(vm);
        }

        var auth = await _api.ReadAsync<AuthResponse>(response);
        string? apiCookie = RaceDayApiClient.ExtractSessionCookie(response);

        if (auth == null || string.IsNullOrWhiteSpace(apiCookie))
        {
            // TEMPORARY DEBUG MESSAGE (we will put the friendly message back once it works)
            string headerNames = string.Join(", ", response.Headers.Select(h => h.Key));
            ModelState.AddModelError("", "Login worked but the session could not be started.");
            return View(vm);
        }

        // Remember who is logged in (MVC session) and the API's cookie.
        HttpContext.Session.SetString("ApiSessionCookie", apiCookie);
        HttpContext.Session.SetInt32("UserId", auth.UserID);
        HttpContext.Session.SetString("Role", auth.Role);
        HttpContext.Session.SetString("DisplayName", auth.FirstName ?? auth.Email ?? "Member");

        if (string.Equals(auth.Role, "Participant", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "Participant");

        return RedirectToAction("Index", "Organiser");
    }

    // ---------- LOGOUT ----------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try { await _api.SendAsync(HttpMethod.Post, "api/auth/logout"); }
        catch (HttpRequestException) { /* still clear our own session below */ }

        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    // Reads {"message": "..."} from the API so users see a friendly sentence.
    private async Task<string> GetFriendlyError(HttpResponseMessage response, string fallback)
    {
        try
        {
            string json = await response.Content.ReadAsStringAsync();
            var msg = System.Text.Json.JsonSerializer.Deserialize<ApiMessage>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return string.IsNullOrWhiteSpace(msg?.Message) ? fallback : msg.Message;
        }
        catch
        {
            return fallback;
        }
    }
}