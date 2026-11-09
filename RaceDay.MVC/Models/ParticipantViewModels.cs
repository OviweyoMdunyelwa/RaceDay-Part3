namespace RaceDay.MVC.Models;

public class EventListVm
{
    public List<EventDto> Events { get; set; } = new();
    public string? Type { get; set; }
    public string? Search { get; set; }
}

public class EventDetailsVm
{
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public bool AlreadyEnrolled { get; set; }
}

public class EnrolmentRowVm
{
    public EnrolmentDto Enrolment { get; set; } = new();
    public EventDto? Event { get; set; }
    public CategoryDto? Category { get; set; }
}