using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        // Both roles can view events.
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events.ToListAsync();
            return Ok(events);
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            return Ok(eventItem);
        }

        // POST: api/Events
        // Organiser only.
        [HttpPost]
        public async Task<IActionResult> CreateEvent(Event eventItem)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null || role == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can create events."
                });
            }

            if (string.IsNullOrWhiteSpace(eventItem.EventName))
            {
                return BadRequest(new
                {
                    message = "Event name is required."
                });
            }

            if (string.IsNullOrWhiteSpace(eventItem.Location))
            {
                return BadRequest(new
                {
                    message = "Event location is required."
                });
            }

            if (eventItem.EventDate == default ||
                eventItem.RegistrationDeadline == default)
            {
                return BadRequest(new
                {
                    message = "Valid event and registration dates are required."
                });
            }

            if (eventItem.DistanceKM <= 0)
            {
                return BadRequest(new
                {
                    message = "Event distance must be greater than zero."
                });
            }

            if (!IsValidEventType(eventItem.EventType))
            {
                return BadRequest(new
                {
                    message = "Event type must be Run, Walk, or Cycle."
                });
            }

            eventItem.EventName = eventItem.EventName.Trim();
            eventItem.Location = eventItem.Location.Trim();
            eventItem.EventType = eventItem.EventType.Trim();

            // Use the logged-in organiser.
            eventItem.OrganiserID = userID.Value;

            // Set creation date automatically.
            eventItem.CreatedAt = DateTime.UtcNow;

            // IMPORTANT: Prevent EF Core from inserting
            // the nested organiser as a new database user.
            eventItem.Organiser = null;

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventItem.EventID },
                eventItem);
        }

        // PUT: api/Events/5
        // Organiser can update only their own events.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            Event updatedEvent)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null || role == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can update events."
                });
            }

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (existingEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (existingEvent.OrganiserID != userID.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only update your own events."
                });
            }

            if (string.IsNullOrWhiteSpace(updatedEvent.EventName) ||
                string.IsNullOrWhiteSpace(updatedEvent.Location))
            {
                return BadRequest(new
                {
                    message = "Event name and location are required."
                });
            }

            if (updatedEvent.EventDate == default ||
                updatedEvent.RegistrationDeadline == default)
            {
                return BadRequest(new
                {
                    message = "Valid event and registration dates are required."
                });
            }

            if (updatedEvent.DistanceKM <= 0)
            {
                return BadRequest(new
                {
                    message = "Event distance must be greater than zero."
                });
            }

            if (!IsValidEventType(updatedEvent.EventType))
            {
                return BadRequest(new
                {
                    message = "Event type must be Run, Walk, or Cycle."
                });
            }

            existingEvent.EventName = updatedEvent.EventName.Trim();
            existingEvent.Description = updatedEvent.Description;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.Location = updatedEvent.Location.Trim();
            existingEvent.DistanceKM = updatedEvent.DistanceKM;
            existingEvent.EventType = updatedEvent.EventType.Trim();
            existingEvent.RegistrationDeadline = updatedEvent.RegistrationDeadline;
            existingEvent.Status = updatedEvent.Status;

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        // DELETE: api/Events/5
        // Organiser can delete only their own events.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null || role == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can delete events."
                });
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (eventItem.OrganiserID != userID.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only delete your own events."
                });
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }

        private static bool IsValidEventType(string? eventType)
        {
            return eventType?.Trim().ToLowerInvariant() is
                "run" or "walk" or "cycle";
        }
    }
}
