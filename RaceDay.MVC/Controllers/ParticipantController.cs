using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[RoleRequired("Participant")]
public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;

    public ParticipantController(RaceDayApiClient api)
    {
        _api = api;
    }

    // If the API has forgotten us (session timed out), send the user to Login.
    private IActionResult? SessionExpired(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.Unauthorized) return null;

        HttpContext.Session.Clear();
        TempData["Error"] = "Your session expired. Please log in again.";
        return RedirectToAction("Login", "Auth");
    }

    private async Task<List<T>> GetListAsync<T>(string url)
    {
        var response = await _api.SendAsync(HttpMethod.Get, url);
        return await _api.ReadAsync<List<T>>(response) ?? new List<T>();
    }

    // ---------- UPCOMING EVENTS (with filters) ----------
    public async Task<IActionResult> Index(string? type, string? search)
    {
        var events = await GetListAsync<EventDto>("api/events");

        events = events
            .Where(e => e.EventDate.Date >= DateTime.Today)
            .OrderBy(e => e.EventDate)
            .ToList();

        if (!string.IsNullOrWhiteSpace(type))
        {
            events = events
                .Where(e => string.Equals(e.EventType, type, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            events = events.Where(e =>
                e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return View(new EventListVm { Events = events, Type = type, Search = search });
    }

    // ---------- EVENT DETAILS + CATEGORIES ----------
    public async Task<IActionResult> Details(int id)
    {
        var eventResponse = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        var ev = await _api.ReadAsync<EventDto>(eventResponse);

        if (ev == null)
        {
            TempData["Error"] = "That event could not be found.";
            return RedirectToAction(nameof(Index));
        }

        var categories = (await GetListAsync<CategoryDto>("api/categories"))
            .Where(c => c.EventID == id)
            .ToList();

        var enrolResponse = await _api.SendAsync(HttpMethod.Get, "api/enrolments");
        if (SessionExpired(enrolResponse) is { } expired) return expired;
        var myEnrolments = await _api.ReadAsync<List<EnrolmentDto>>(enrolResponse) ?? new();

        return View(new EventDetailsVm
        {
            Event = ev,
            Categories = categories,
            AlreadyEnrolled = myEnrolments.Any(e => e.EventID == id)
        });
    }

    // ---------- ENTER AN EVENT ----------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterEvent(int eventId, int? categoryId)
    {
        if (categoryId == null)
        {
            TempData["Error"] = "Please choose a category first.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        var response = await _api.SendAsync(HttpMethod.Post, "api/enrolments",
            new CreateEnrolmentRequest { EventID = eventId, CategoryID = categoryId.Value });

        if (SessionExpired(response) is { } expired) return expired;

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await ApiErrorReader.ReadAsync(response, "Could not enter the event.");
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        TempData["Success"] = "You have entered the event!";
        return RedirectToAction(nameof(MyEvents));
    }

    // ---------- MY ENROLMENTS ----------
    public async Task<IActionResult> MyEvents()
    {
        var enrolResponse = await _api.SendAsync(HttpMethod.Get, "api/enrolments");
        if (SessionExpired(enrolResponse) is { } expired) return expired;

        var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(enrolResponse) ?? new();
        var events = await GetListAsync<EventDto>("api/events");
        var categories = await GetListAsync<CategoryDto>("api/categories");

        var rows = enrolments
            .OrderByDescending(e => e.EnrolmentDate)
            .Select(e => new EnrolmentRowVm
            {
                Enrolment = e,
                Event = events.FirstOrDefault(x => x.EventID == e.EventID),
                Category = categories.FirstOrDefault(c => c.CategoryID == e.CategoryID)
            })
            .ToList();

        return View(rows);
    }

    // ---------- CANCEL AN ENROLMENT ----------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var response = await _api.SendAsync(HttpMethod.Delete, $"api/enrolments/{id}");

        if (SessionExpired(response) is { } expired) return expired;

        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
            response.IsSuccessStatusCode
                ? "Your enrolment was cancelled."
                : await ApiErrorReader.ReadAsync(response, "Could not cancel the enrolment.");

        return RedirectToAction(nameof(MyEvents));
    }

    // ---------- MY RESULTS ----------
    public async Task<IActionResult> MyResults()
    {
        var resultResponse = await _api.SendAsync(HttpMethod.Get, "api/results");
        if (SessionExpired(resultResponse) is { } expired) return expired;

        var results = (await _api.ReadAsync<List<ResultDto>>(resultResponse) ?? new())
    .Where(r => r.Status == ResultStatuses.Published)
    .ToList();
        var enrolments = await GetListAsync<EnrolmentDto>("api/enrolments");
        var events = await GetListAsync<EventDto>("api/events");
        var categories = await GetListAsync<CategoryDto>("api/categories");

        var rows = results.Select(r =>
        {
            var enrolment = enrolments.FirstOrDefault(e => e.EnrolmentID == r.EnrolmentID);
            return new MyResultRowVm
            {
                Result = r,
                Event = enrolment == null ? null : events.FirstOrDefault(x => x.EventID == enrolment.EventID),
                Category = enrolment == null ? null : categories.FirstOrDefault(c => c.CategoryID == enrolment.CategoryID)
            };
        })
        .OrderByDescending(r => r.Event?.EventDate)
        .ToList();

        return View(rows);
    }

    // ---------- PROFILE ----------
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var response = await _api.SendAsync(HttpMethod.Get, "api/users/profile");
        if (SessionExpired(response) is { } expired) return expired;

        var profile = await _api.ReadAsync<ProfileDto>(response);
        if (profile == null)
        {
            TempData["Error"] = "Your profile could not be loaded.";
            return RedirectToAction(nameof(Index));
        }

        return View(new ProfileVm
        {
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            PhoneNumber = profile.PhoneNumber,
            Email = profile.Email
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var response = await _api.SendAsync(HttpMethod.Put, "api/users/profile",
            new UpdateProfileRequest
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                PhoneNumber = vm.PhoneNumber
            });

        if (SessionExpired(response) is { } expired) return expired;

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ApiErrorReader.ReadAsync(response, "Could not update your profile."));
            return View(vm);
        }

        // Keep the name in the menu up to date.
        HttpContext.Session.SetString("DisplayName", vm.FirstName.Trim());

        TempData["Success"] = "Your profile was updated.";
        return RedirectToAction(nameof(Profile));
    }
}