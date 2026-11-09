using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

// Shows pictures by asking the API for them (the website never talks to Azure).
public class MediaController : Controller
{
    private readonly RaceDayApiClient _api;

    public MediaController(RaceDayApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Image(string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            path.Contains("..") ||
            !(path.StartsWith("event-banners/") || path.StartsWith("profile-images/")))
        {
            return NotFound();
        }

        HttpResponseMessage response;
        try
        {
            response = await _api.SendAsync(HttpMethod.Get, "api/media/" + path);
        }
        catch (HttpRequestException)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode) return NotFound();

        byte[] bytes = await response.Content.ReadAsByteArrayAsync();
        string contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

        Response.Headers.CacheControl = "public,max-age=86400";
        return File(bytes, contentType);
    }
}