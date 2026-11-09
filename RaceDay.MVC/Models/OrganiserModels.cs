using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RaceDay.MVC.Models;

// The words used for a result's Status. If your database uses different words, change them HERE only.
public static class ResultStatuses
{
    public const string Draft = "Provisional";   // organiser can see it, participant cannot
    public const string Published = "Official";  // participant can see it
}

// ---------- JSON shapes from / to the API ----------

public class OrganiserEnrolmentDto
{
    public int EnrolmentID { get; set; }
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantEmail { get; set; } = string.Empty;
    public string? ParticipantPhone { get; set; }
    public string? RaceNumber { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public int? ResultID { get; set; }
}

public class EventRequest
{
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKM { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime RegistrationDeadline { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CategoryRequest
{
    public int EventID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal DistanceKM { get; set; }
    public decimal EntryFee { get; set; }
    public string? AgeGroup { get; set; }
    public string? Description { get; set; }
}

public class ResultRequest
{
    public int EnrolmentID { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public TimeSpan? ChipTime { get; set; }
    public int? PositionOverall { get; set; }
    public int? PositionCategory { get; set; }
    public decimal? Pace { get; set; }
    public string Status { get; set; } = string.Empty;
}

// ---------- Form models ----------

public class EventFormVm
{
    public int EventID { get; set; }

    [Required, Display(Name = "Event name"), MaxLength(100)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Event date")]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);

    [Required, MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required, Range(0.1, 999.99, ErrorMessage = "Distance must be between 0.1 and 999.99 km."), Display(Name = "Distance (km)")]
    public decimal DistanceKM { get; set; }

    [Required, Display(Name = "Event type")]
    public string EventType { get; set; } = "Run";

    [Required, DataType(DataType.Date), Display(Name = "Registration deadline")]
    public DateTime RegistrationDeadline { get; set; } = DateTime.Today.AddDays(23);

    [Required]
    public string Status { get; set; } = string.Empty;

    [ValidateNever]
    public List<string> StatusOptions { get; set; } = new();
}

public class CategoryFormVm
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }

    [Required, MaxLength(100), Display(Name = "Category name")]
    public string CategoryName { get; set; } = string.Empty;

    [Required, Range(0.1, 999.99, ErrorMessage = "Distance must be between 0.1 and 999.99 km."), Display(Name = "Distance (km)")]
    public decimal DistanceKM { get; set; }

    [Required, Range(0, 99999999.99, ErrorMessage = "Entry fee cannot be negative."), Display(Name = "Entry fee (R)")]
    public decimal EntryFee { get; set; }

    [MaxLength(50), Display(Name = "Age group")]
    public string? AgeGroup { get; set; }

    [MaxLength(300)]
    public string? Description { get; set; }
}

public class ResultFormVm
{
    public int EnrolmentID { get; set; }
    public int? ResultID { get; set; }

    // Shown on the page only (filled in by the controller).
    public string? ParticipantName { get; set; }
    public string? EventName { get; set; }
    public string? CategoryName { get; set; }
    public string? RaceNumber { get; set; }

    [Required(ErrorMessage = "Enter the finish time."), Display(Name = "Finish time (hh:mm:ss)")]
    public string? FinishTimeText { get; set; }

    [Required(ErrorMessage = "Enter the finishing position."), Range(1, 1000000), Display(Name = "Overall position")]
    public int? PositionOverall { get; set; }

    [Range(1, 1000000), Display(Name = "Category position")]
    public int? PositionCategory { get; set; }

    [Required]
    public string Status { get; set; } = ResultStatuses.Draft;
}

// ---------- Page models ----------

public class DashboardEventRowVm
{
    public EventDto Event { get; set; } = new();
    public int EnrolmentCount { get; set; }
    public int DaysAway { get; set; }
}

public class OrganiserDashboardVm
{
    public int TotalEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int TotalEnrolments { get; set; }
    public int PendingEnrolments { get; set; }
    public List<DashboardEventRowVm> Upcoming { get; set; } = new();
    public List<OrganiserEnrolmentDto> RecentEnrolments { get; set; } = new();
}

public class CategoriesPageVm
{
    public List<EventDto> Events { get; set; } = new();
    public EventDto? SelectedEvent { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
    public CategoryFormVm NewCategory { get; set; } = new();
}

public class EnrolmentsPageVm
{
    public List<EventDto> Events { get; set; } = new();
    public int? SelectedEventId { get; set; }
    public List<OrganiserEnrolmentDto> Enrolments { get; set; } = new();
}

public class OrganiserResultRowVm
{
    public ResultDto Result { get; set; } = new();
    public OrganiserEnrolmentDto? Enrolment { get; set; }
}

public class ResultsPageVm
{
    public List<OrganiserResultRowVm> Rows { get; set; } = new();
    public List<OrganiserEnrolmentDto> AwaitingResult { get; set; } = new();
}
