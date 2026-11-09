namespace RaceDay.MVC.Models;

// Shapes of the JSON the API returns. NOT database classes.

public class EventDto
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKM { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime RegistrationDeadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? BannerImageUrl { get; set; }
}

public class CategoryDto
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal DistanceKM { get; set; }
    public decimal EntryFee { get; set; }
    public string? AgeGroup { get; set; }
    public string? Description { get; set; }
}

public class EnrolmentDto
{
    public int EnrolmentID { get; set; }
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    public int ParticipantID { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string? RaceNumber { get; set; }
    public string? PaymentStatus { get; set; }
}

public class CreateEnrolmentRequest
{
    public int EventID { get; set; }
    public int CategoryID { get; set; }
}