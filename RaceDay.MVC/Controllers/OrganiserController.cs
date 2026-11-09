using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[RoleRequired("Organiser")]
public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;

    public OrganiserController(RaceDayApiClient api)
    {
        _api = api;
    }

    private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

    // ---------- helpers ----------

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

    // Only the events owned by the logged-in organiser.
    private async Task<List<EventDto>> GetMyEventsAsync()
    {
        var all = await GetListAsync<EventDto>("api/events");
        return all.Where(e => e.OrganiserID == CurrentUserId)
                  .OrderBy(e => e.EventDate)
                  .ToList();
    }

    private async Task<EventDto?> GetMyEventAsync(int id)
    {
        var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        var ev = await _api.ReadAsync<EventDto>(response);
        return ev != null && ev.OrganiserID == CurrentUserId ? ev : null;
    }

    private async Task<(List<OrganiserEnrolmentDto> Items, IActionResult? Expired)> GetEnrolmentsAsync()
    {
        var response = await _api.SendAsync(HttpMethod.Get, "api/organiser/enrolments");
        var expired = SessionExpired(response);
        var items = await _api.ReadAsync<List<OrganiserEnrolmentDto>>(response) ?? new();
        return (items, expired);
    }

    // The status words an event may use: the ones already in the database.
    private async Task<List<string>> GetStatusOptionsAsync()
    {
        var all = await GetListAsync<EventDto>("api/events");
        var statuses = all.Select(e => e.Status)
                          .Where(s => !string.IsNullOrWhiteSpace(s))
                          .Distinct()
                          .ToList();

        return statuses.Count > 0
            ? statuses
            : new List<string> { "Open", "Closed", "Cancelled", "Completed" };
    }

    private void ValidateDates(EventFormVm vm)
    {
        if (vm.EventDate.Year < 2000)
            ModelState.AddModelError(nameof(vm.EventDate), "Please choose the event date.");

        if (vm.RegistrationDeadline.Year < 2000)
            ModelState.AddModelError(nameof(vm.RegistrationDeadline), "Please choose the registration deadline.");
        else if (vm.RegistrationDeadline.Date > vm.EventDate.Date)
            ModelState.AddModelError(nameof(vm.RegistrationDeadline), "The registration deadline must be on or before the event date.");
    }

    private static EventRequest ToEventRequest(EventFormVm vm) => new()
    {
        EventName = vm.EventName.Trim(),
        Description = vm.Description,
        EventDate = vm.EventDate.Date,
        Location = vm.Location.Trim(),
        DistanceKM = vm.DistanceKM,
        EventType = vm.EventType,
        RegistrationDeadline = vm.RegistrationDeadline.Date,
        Status = vm.Status
    };

    private string FirstError() =>
        ModelState.Values.SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
        ?? "Please check the form and try again.";

    // ---------- DASHBOARD ----------

    public async Task<IActionResult> Index()
    {
        var (enrolments, expired) = await GetEnrolmentsAsync();
        if (expired != null) return expired;

        var events = await GetMyEventsAsync();
        var upcoming = events.Where(e => e.EventDate.Date >= DateTime.Today).ToList();

        var vm = new OrganiserDashboardVm
        {
            TotalEvents = events.Count,
            UpcomingEvents = upcoming.Count,
            TotalEnrolments = enrolments.Count,
            PendingEnrolments = enrolments.Count(e =>
                string.Equals(e.PaymentStatus, "Pending", StringComparison.OrdinalIgnoreCase)),
            Upcoming = upcoming.Take(5).Select(e => new DashboardEventRowVm
            {
                Event = e,
                EnrolmentCount = enrolments.Count(x => x.EventID == e.EventID),
                DaysAway = (e.EventDate.Date - DateTime.Today).Days
            }).ToList(),
            RecentEnrolments = enrolments.OrderByDescending(e => e.EnrolmentDate).Take(5).ToList()
        };

        return View(vm);
    }

    // ---------- EVENTS ----------

    public async Task<IActionResult> Events()
    {
        var (enrolments, expired) = await GetEnrolmentsAsync();
        if (expired != null) return expired;

        var events = await GetMyEventsAsync();

        var rows = events.Select(e => new DashboardEventRowVm
        {
            Event = e,
            EnrolmentCount = enrolments.Count(x => x.EventID == e.EventID),
            DaysAway = (e.EventDate.Date - DateTime.Today).Days
        }).ToList();

        return View(rows);
    }

    [HttpGet]
    public async Task<IActionResult> CreateEvent()
    {
        var options = await GetStatusOptionsAsync();

        return View(new EventFormVm
        {
            StatusOptions = options,
            Status = options.First()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventFormVm vm)
    {
        vm.StatusOptions = await GetStatusOptionsAsync();
        ValidateDates(vm);
        if (!ModelState.IsValid) return View(vm);

        var response = await _api.SendAsync(HttpMethod.Post, "api/events", ToEventRequest(vm));

        var expired = SessionExpired(response);
        if (expired != null) return expired;

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ApiErrorReader.ReadAsync(response, "The event could not be created."));
            return View(vm);
        }

        var created = await _api.ReadAsync<EventDto>(response);
        TempData["Success"] = "Event created. Add categories so participants can enter.";

        return created == null
            ? RedirectToAction(nameof(Events))
            : RedirectToAction(nameof(Categories), new { eventId = created.EventID });
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var ev = await GetMyEventAsync(id);
        if (ev == null)
        {
            TempData["Error"] = "That event could not be found.";
            return RedirectToAction(nameof(Events));
        }

        var options = await GetStatusOptionsAsync();
        if (!options.Contains(ev.Status)) options.Add(ev.Status);

        return View(new EventFormVm
        {
            EventID = ev.EventID,
            EventName = ev.EventName,
            Description = ev.Description,
            EventDate = ev.EventDate.Date,
            Location = ev.Location,
            DistanceKM = ev.DistanceKM,
            EventType = ev.EventType,
            RegistrationDeadline = ev.RegistrationDeadline.Date,
            Status = ev.Status,
            StatusOptions = options
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(EventFormVm vm)
    {
        vm.StatusOptions = await GetStatusOptionsAsync();
        if (!vm.StatusOptions.Contains(vm.Status)) vm.StatusOptions.Add(vm.Status);

        ValidateDates(vm);
        if (!ModelState.IsValid) return View(vm);

        var response = await _api.SendAsync(HttpMethod.Put, $"api/events/{vm.EventID}", ToEventRequest(vm));

        var expired = SessionExpired(response);
        if (expired != null) return expired;

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ApiErrorReader.ReadAsync(response, "The event could not be updated."));
            return View(vm);
        }

        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(Events));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var response = await _api.SendAsync(HttpMethod.Delete, $"api/events/{id}");

        var expired = SessionExpired(response);
        if (expired != null) return expired;

        if (response.IsSuccessStatusCode)
            TempData["Success"] = "Event deleted.";
        else
            TempData["Error"] = await ApiErrorReader.ReadAsync(response,
                "The event could not be deleted. Delete its categories and enrolments first.");

        return RedirectToAction(nameof(Events));
    }

    // ---------- CATEGORIES ----------

    public async Task<IActionResult> Categories(int? eventId)
    {
        var events = await GetMyEventsAsync();
        var selected = events.FirstOrDefault(e => e.EventID == eventId) ?? events.FirstOrDefault();

        var vm = new CategoriesPageVm { Events = events, SelectedEvent = selected };

        if (selected != null)
        {
            vm.Categories = (await GetListAsync<CategoryDto>("api/categories"))
                .Where(c => c.EventID == selected.EventID)
                .ToList();
            vm.NewCategory.EventID = selected.EventID;
        }

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory([Bind(Prefix = "NewCategory")] CategoryFormVm vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = FirstError();
            return RedirectToAction(nameof(Categories), new { eventId = vm.EventID });
        }

        var response = await _api.SendAsync(HttpMethod.Post, "api/categories", new CategoryRequest
        {
            EventID = vm.EventID,
            CategoryName = vm.CategoryName.Trim(),
            DistanceKM = vm.DistanceKM,
            EntryFee = vm.EntryFee,
            AgeGroup = vm.AgeGroup,
            Description = vm.Description
        });

        var expired = SessionExpired(response);
        if (expired != null) return expired;

        if (response.IsSuccessStatusCode)
            TempData["Success"] = "Category added.";
        else
            TempData["Error"] = await ApiErrorReader.ReadAsync(response, "The category could not be added.");

        return RedirectToAction(nameof(Categories), new { eventId = vm.EventID });
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        var response = await _api.SendAsync(HttpMethod.Get, $"api/categories/{id}");
        var category = await _api.ReadAsync<CategoryDto>(response);
        var myEvents = await GetMyEventsAsync();

        if (category == null || !myEvents.Any(e => e.EventID == category.EventID))
        {
            TempData["Error"] = "That category could not be found.";
            return RedirectToAction(nameof(Categories));
        }

        return View(new CategoryFormVm
        {
            CategoryID = category.CategoryID,
            EventID = category.EventID,
            CategoryName = category.CategoryName,
            DistanceKM = category.DistanceKM,
            EntryFee = category.EntryFee,
            AgeGroup = category.AgeGroup,
            Description = category.Description
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(CategoryFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var response = await _api.SendAsync(HttpMethod.Put, $"api/categories/{vm.CategoryID}", new CategoryRequest
        {
            EventID = vm.EventID,
            CategoryName = vm.CategoryName.Trim(),
            DistanceKM = vm.DistanceKM,
            EntryFee = vm.EntryFee,
            AgeGroup = vm.AgeGroup,
            Description = vm.Description
        });

        var expired = SessionExpired(response);
        if (expired != null) return expired;

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ApiErrorReader.ReadAsync(response, "The category could not be updated."));
            return View(vm);
        }

        TempData["Success"] = "Category updated.";
        return RedirectToAction(nameof(Categories), new { eventId = vm.EventID });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int eventId)
    {
        var response = await _api.SendAsync(HttpMethod.Delete, $"api/categories/{id}");

        var expired = SessionExpired(response);
        if (expired != null) return expired;

        if (response.IsSuccessStatusCode)
            TempData["Success"] = "Category deleted.";
        else
            TempData["Error"] = await ApiErrorReader.ReadAsync(response,
                "The category could not be deleted. Participants may already be enrolled in it.");

        return RedirectToAction(nameof(Categories), new { eventId });
    }

    // ---------- ENROLMENTS ----------

    public async Task<IActionResult> Enrolments(int? eventId)
    {
        var (enrolments, expired) = await GetEnrolmentsAsync();
        if (expired != null) return expired;

        var events = await GetMyEventsAsync();

        if (eventId.HasValue)
            enrolments = enrolments.Where(e => e.EventID == eventId.Value).ToList();

        return View(new EnrolmentsPageVm
        {
            Events = events,
            SelectedEventId = eventId,
            Enrolments = enrolments
        });
    }

    // ---------- RESULTS ----------

    public async Task<IActionResult> Results()
    {
        var (enrolments, expired) = await GetEnrolmentsAsync();
        if (expired != null) return expired;

        var results = await GetListAsync<ResultDto>("api/results");

        var rows = results.Select(r => new OrganiserResultRowVm
        {
            Result = r,
            Enrolment = enrolments.FirstOrDefault(e => e.EnrolmentID == r.EnrolmentID)
        }).ToList();

        return View(new ResultsPageVm
        {
            Rows = rows,
            AwaitingResult = enrolments.Where(e => e.ResultID == null).ToList()
        });
    }

    private static ResultFormVm BuildResultForm(OrganiserEnrolmentDto enrolment) => new()
    {
        EnrolmentID = enrolment.EnrolmentID,
        ParticipantName = enrolment.ParticipantName,
        EventName = enrolment.EventName,
        CategoryName = enrolment.CategoryName,
        RaceNumber = enrolment.RaceNumber
    };

    [HttpGet]
    public async Task<IActionResult> CaptureResult(int enrolmentId)
    {
        var (enrolments, expired) = await GetEnrolmentsAsync();
        if (expired != null) return expired;

        var enrolment = enrolments.FirstOrDefault(e => e.EnrolmentID == enrolmentId);
        if (enrolment == null)
        {
            TempData["Error"] = "That enrolment could not be found.";
            return RedirectToAction(nameof(Results));
        }

        var vm = BuildResultForm(enrolment);

        if (enrolment.ResultID.HasValue)
        {
            var response = await _api.SendAsync(HttpMethod.Get, $"api/results/{enrolment.ResultID.Value}");
            var existing = await _api.ReadAsync<ResultDto>(response);

            if (existing != null)
            {
                vm.ResultID = existing.ResultID;
                vm.FinishTimeText = existing.FinishTime?.ToString(@"hh\:mm\:ss");
                vm.PositionOverall = existing.PositionOverall;
                vm.PositionCategory = existing.PositionCategory;
                vm.Status = existing.Status;
            }
        }

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CaptureResult(ResultFormVm vm)
    {
        var (enrolments, expired) = await GetEnrolmentsAsync();
        if (expired != null) return expired;

        var enrolment = enrolments.FirstOrDefault(e => e.EnrolmentID == vm.EnrolmentID);
        if (enrolment == null)
        {
            TempData["Error"] = "That enrolment could not be found.";
            return RedirectToAction(nameof(Results));
        }

        // Refill the text shown at the top of the page.
        vm.ParticipantName = enrolment.ParticipantName;
        vm.EventName = enrolment.EventName;
        vm.CategoryName = enrolment.CategoryName;
        vm.RaceNumber = enrolment.RaceNumber;

        bool timeOk = TimeSpan.TryParse(vm.FinishTimeText, CultureInfo.InvariantCulture, out TimeSpan finish);
        if (!timeOk || finish <= TimeSpan.Zero)
            ModelState.AddModelError(nameof(vm.FinishTimeText), "Enter the time like 01:25:30 (hours:minutes:seconds).");

        if (!ModelState.IsValid) return View(vm);

        // Pace = minutes per kilometre, worked out from the category distance.
        var categories = await GetListAsync<CategoryDto>("api/categories");
        var category = categories.FirstOrDefault(c => c.CategoryID == enrolment.CategoryID);
        decimal? pace = null;
        if (category != null && category.DistanceKM > 0)
            pace = Math.Min(999.99m, Math.Round((decimal)finish.TotalMinutes / category.DistanceKM, 2));

        var request = new ResultRequest
        {
            EnrolmentID = vm.EnrolmentID,
            FinishTime = finish,
            PositionOverall = vm.PositionOverall,
            PositionCategory = vm.PositionCategory,
            Pace = pace,
            Status = vm.Status
        };

        HttpResponseMessage response = vm.ResultID.HasValue
            ? await _api.SendAsync(HttpMethod.Put, $"api/results/{vm.ResultID.Value}", request)
            : await _api.SendAsync(HttpMethod.Post, "api/results", request);

        var expired2 = SessionExpired(response);
        if (expired2 != null) return expired2;

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ApiErrorReader.ReadAsync(response,
                "The result could not be saved. Please check the details and try again."));
            return View(vm);
        }

        TempData["Success"] = vm.Status == ResultStatuses.Published
            ? "Result saved and published."
            : "Result saved as a draft (the participant cannot see it yet).";

        return RedirectToAction(nameof(Results));
    }

    // Publish or unpublish: changes only the status of an existing result.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetResultStatus(int id, string status)
    {
        var getResponse = await _api.SendAsync(HttpMethod.Get, $"api/results/{id}");
        var existing = await _api.ReadAsync<ResultDto>(getResponse);

        if (existing == null)
        {
            TempData["Error"] = "That result could not be found.";
            return RedirectToAction(nameof(Results));
        }

        var response = await _api.SendAsync(HttpMethod.Put, $"api/results/{id}", new ResultRequest
        {
            EnrolmentID = existing.EnrolmentID,
            FinishTime = existing.FinishTime,
            ChipTime = existing.ChipTime,
            PositionOverall = existing.PositionOverall,
            PositionCategory = existing.PositionCategory,
            Pace = existing.Pace,
            Status = status
        });

        var expired = SessionExpired(response);
        if (expired != null) return expired;

        if (response.IsSuccessStatusCode)
            TempData["Success"] = status == ResultStatuses.Published ? "Result published." : "Result unpublished.";
        else
            TempData["Error"] = await ApiErrorReader.ReadAsync(response, "The result could not be updated.");

        return RedirectToAction(nameof(Results));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResult(int id)
    {
        var response = await _api.SendAsync(HttpMethod.Delete, $"api/results/{id}");

        var expired = SessionExpired(response);
        if (expired != null) return expired;

        if (response.IsSuccessStatusCode)
            TempData["Success"] = "Result deleted.";
        else
            TempData["Error"] = await ApiErrorReader.ReadAsync(response, "The result could not be deleted.");

        return RedirectToAction(nameof(Results));
    }
}
