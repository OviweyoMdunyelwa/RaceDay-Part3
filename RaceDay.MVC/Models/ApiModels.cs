namespace RaceDay.MVC.Models;

// These classes describe the JSON that the Part 2 API sends and receives.
// They are NOT database classes. MVC never touches the database.

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Participant";
    public string? PhoneNumber { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string? Message { get; set; }
    public int UserID { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class ApiMessage
{
    public string? Message { get; set; }
}