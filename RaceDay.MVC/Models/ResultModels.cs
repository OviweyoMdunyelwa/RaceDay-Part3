using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models;

// JSON shapes from the API (not database classes).

public class ResultDto
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public TimeSpan? ChipTime { get; set; }
    public int? PositionOverall { get; set; }
    public int? PositionCategory { get; set; }
    public decimal? Pace { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ProfileDto
{
    public int UserID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class MyResultRowVm
{
    public ResultDto Result { get; set; } = new();
    public EventDto? Event { get; set; }
    public CategoryDto? Category { get; set; }
}

public class ProfileVm
{
    [Required, Display(Name = "First name"), MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, Display(Name = "Last name"), MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Phone number"), MaxLength(20)]
    public string? PhoneNumber { get; set; }

    // Shown but not editable.
    public string Email { get; set; } = string.Empty;
}
