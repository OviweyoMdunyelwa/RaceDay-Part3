using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganiserController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public OrganiserController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Organiser/enrolments
        // Enrolments (with participant names) for the logged-in organiser's own events.
        [HttpGet("enrolments")]
        public async Task<IActionResult> GetEnrolments()
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new { message = "Only Organisers can view this." });
            }

            var rows = await (
                from e in _context.Enrolments
                join ev in _context.Events on e.EventID equals ev.EventID
                join c in _context.Categories on e.CategoryID equals c.CategoryID
                join u in _context.RaceDayUsers on e.ParticipantID equals u.UserID
                where ev.OrganiserID == userID.Value
                orderby ev.EventDate, e.RaceNumber
                select new
                {
                    e.EnrolmentID,
                    e.EventID,
                    EventName = ev.EventName,
                    e.CategoryID,
                    CategoryName = c.CategoryName,
                    e.ParticipantID,
                    ParticipantName = u.FirstName + " " + u.LastName,
                    ParticipantEmail = u.Email,
                    ParticipantPhone = u.PhoneNumber,
                    e.RaceNumber,
                    e.PaymentStatus,
                    e.EnrolmentDate,
                    ResultID = _context.Results
                        .Where(r => r.EnrolmentID == e.EnrolmentID)
                        .Select(r => (int?)r.ResultID)
                        .FirstOrDefault()
                }).ToListAsync();

            return Ok(rows);
        }
    }
}