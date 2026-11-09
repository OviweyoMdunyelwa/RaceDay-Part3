using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[RoleRequired("Organiser")]
public class BannerController : Controller
{
    private readonly RaceDayApiClient _api;

    public BannerController(RaceDayApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Upload(int id)
    {
        var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        var ev = await _api.ReadAsync<EventDto>(response);
        int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

        if (ev == null || ev.OrganiserID != userId)
        {
            TempData["Error"] = "That event could not be found.";
            return RedirectToAction("Events", "Organiser");
        }

        return View(ev);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int id, IFormFile? banner)
    {
        if (banner == null || banner.Length == 0)
        {
            TempData["Error"] = "Please choose an image first.";
            return RedirectToAction(nameof(Upload), new { id });
        }

        HttpResponseMessage response;
        try
        {
            response = await _api.UploadFileAsync($"api/events/{id}/banner", banner);
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "RaceDay could not reach the server. Please try again.";
            return RedirectToAction(nameof(Upload), new { id });
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            HttpContext.Session.Clear();
            TempData["Error"] = "Your session expired. Please log in again.";
            return RedirectToAction("Login", "Auth");
        }

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await ApiErrorReader.ReadAsync(response, "The banner could not be uploaded.");
            return RedirectToAction(nameof(Upload), new { id });
        }

        TempData["Success"] = "Banner uploaded.";
        return RedirectToAction("Events", "Organiser");
    }
}
