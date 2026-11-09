using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.MVC.Filters;

// Put [RoleRequired("Organiser")] above a controller to protect it.
// Not logged in -> sent to Login. Wrong role -> sent to Access Denied.
public class RoleRequiredAttribute : ActionFilterAttribute
{
    private readonly string _role;

    public RoleRequiredAttribute(string role)
    {
        _role = role;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.GetInt32("UserId").HasValue)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        string? role = session.GetString("Role");
        if (!string.Equals(role, _role, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
        }
    }
}