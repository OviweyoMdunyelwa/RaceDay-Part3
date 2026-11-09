using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        // Organisers see enrolments for their own events.
        // Participants see only their own enrolments.
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            if (role == "Organiser")
            {
                var organiserEnrolments = await _context.Enrolments
                    .Where(e => _context.Events.Any(ev =>
                        ev.EventID == e.EventID &&
                        ev.OrganiserID == userID.Value))
                    .ToListAsync();

                return Ok(organiserEnrolments);
            }

            if (role == "Participant")
            {
                var participantEnrolments = await _context.Enrolments
                    .Where(e => e.ParticipantID == userID.Value)
                    .ToListAsync();

                return Ok(participantEnrolments);
            }

            return StatusCode(403, new
            {
                message = "You do not have permission to view enrolments."
            });
        }

        // GET: api/Enrolments/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            if (role != "Organiser" && role != "Participant")
            {
                return StatusCode(403, new
                {
                    message = "You do not have permission to view enrolments."
                });
            }

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.EnrolmentID == id);

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            if (role == "Participant" &&
                enrolment.ParticipantID != userID.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only view your own enrolments."
                });
            }

            if (role == "Organiser")
            {
                bool ownsEvent = await _context.Events
                    .AnyAsync(ev =>
                        ev.EventID == enrolment.EventID &&
                        ev.OrganiserID == userID.Value);

                if (!ownsEvent)
                {
                    return StatusCode(403, new
                    {
                        message = "You can only view enrolments for your own events."
                    });
                }
            }

            return Ok(enrolment);
        }

        // POST: api/Enrolments
        // Participants can enrol in existing events.
        [HttpPost]
        public async Task<IActionResult> CreateEnrolment(Enrolment enrolment)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            if (role != "Participant")
            {
                return StatusCode(403, new
                {
                    message = "Only Participants can create enrolments."
                });
            }

            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventID == enrolment.EventID);

            if (!eventExists)
            {
                return BadRequest(new
                {
                    message = "The specified event does not exist."
                });
            }

            bool categoryMatchesEvent = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryID == enrolment.CategoryID &&
                    c.EventID == enrolment.EventID);

            if (!categoryMatchesEvent)
            {
                return BadRequest(new
                {
                    message = "The specified category does not exist or does not belong to this event."
                });
            }

            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantID == userID.Value &&
                    e.EventID == enrolment.EventID);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this event."
                });
            }

            // Set ownership and date using the logged-in session.
            enrolment.ParticipantID = userID.Value;
            enrolment.EnrolmentDate = DateTime.UtcNow;

            // The API (not the website) decides the race number and payment status.
            var existingNumbers = await _context.Enrolments
                .Where(e => e.EventID == enrolment.EventID)
                .Select(e => e.RaceNumber)
                .ToListAsync();

            int highest = existingNumbers
                .Select(n => int.TryParse(n, out int value) ? value : 0)
                .DefaultIfEmpty(0)
                .Max();

            enrolment.RaceNumber = (highest + 1).ToString("D4");
            enrolment.PaymentStatus = "Pending";

            // Prevent EF Core from inserting related records.
            enrolment.Event = null;
            enrolment.Category = null;
            enrolment.Participant = null;

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentID },
                enrolment);
        }

        // PUT: api/Enrolments/1
        // Participants can update only their own enrolments.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnrolment(
            int id,
            Enrolment updatedEnrolment)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            if (role != "Participant")
            {
                return StatusCode(403, new
                {
                    message = "Only Participants can update enrolments."
                });
            }

            var existingEnrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.EnrolmentID == id);

            if (existingEnrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            if (existingEnrolment.ParticipantID != userID.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only update your own enrolments."
                });
            }

            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventID == updatedEnrolment.EventID);

            if (!eventExists)
            {
                return BadRequest(new
                {
                    message = "The specified event does not exist."
                });
            }

            bool categoryMatchesEvent = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryID == updatedEnrolment.CategoryID &&
                    c.EventID == updatedEnrolment.EventID);

            if (!categoryMatchesEvent)
            {
                return BadRequest(new
                {
                    message = "The specified category does not exist or does not belong to this event."
                });
            }

            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantID == userID.Value &&
                    e.EventID == updatedEnrolment.EventID &&
                    e.EnrolmentID != id);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this event."
                });
            }

            // Only allow changes to the selected event and category.
            existingEnrolment.EventID = updatedEnrolment.EventID;
            existingEnrolment.CategoryID = updatedEnrolment.CategoryID;

            await _context.SaveChangesAsync();

            return Ok(existingEnrolment);
        }

        // DELETE: api/Enrolments/1
        // Participants can delete only their own enrolments.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            if (role != "Participant")
            {
                return StatusCode(403, new
                {
                    message = "Only Participants can delete enrolments."
                });
            }

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.EnrolmentID == id);

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            if (enrolment.ParticipantID != userID.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only delete your own enrolments."
                });
            }

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment deleted successfully."
            });
        }
    }
}