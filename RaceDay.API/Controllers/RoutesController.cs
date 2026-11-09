using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public RoutesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Checks whether an event belongs to the logged-in organiser.
        private async Task<bool> IsEventOwnedByOrganiser(
            int eventID,
            int organiserID)
        {
            return await _context.Events.AnyAsync(e =>
                e.EventID == eventID &&
                e.OrganiserID == organiserID);
        }

        // GET: api/Routes
        // Organisers see routes for their own events.
        // Participants can view all routes.
        [HttpGet]
        public async Task<IActionResult> GetRoutes()
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

            if (role == "Organiser")
            {
                var organiserRoutes = await _context.Routes
                    .Where(r => _context.Events.Any(e =>
                        e.EventID == r.EventID &&
                        e.OrganiserID == userID.Value))
                    .ToListAsync();

                return Ok(organiserRoutes);
            }

            if (role == "Participant")
            {
                var routes = await _context.Routes.ToListAsync();
                return Ok(routes);
            }

            return StatusCode(403, new
            {
                message = "You do not have permission to view routes."
            });
        }

        // GET: api/Routes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoute(int id)
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

            var route = await _context.Routes
                .FirstOrDefaultAsync(r => r.RouteID == id);

            if (route == null)
            {
                return NotFound(new
                {
                    message = "Route not found."
                });
            }

            if (role == "Organiser")
            {
                bool ownsEvent = await IsEventOwnedByOrganiser(
                    route.EventID,
                    userID.Value);

                if (!ownsEvent)
                {
                    return StatusCode(403, new
                    {
                        message = "You can only view routes for your own events."
                    });
                }
            }
            else if (role != "Participant")
            {
                return StatusCode(403, new
                {
                    message = "You do not have permission to view routes."
                });
            }

            return Ok(route);
        }

        // POST: api/Routes
        // Organisers can create routes for their own events only.
        [HttpPost]
        public async Task<IActionResult> CreateRoute(
            RaceDay.API.Models.Route route)
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
                    message = "Only Organisers can create routes."
                });
            }

            bool ownsEvent = await IsEventOwnedByOrganiser(
                route.EventID,
                userID.Value);

            if (!ownsEvent)
            {
                return StatusCode(403, new
                {
                    message = "You can only create routes for your own events."
                });
            }

            _context.Routes.Add(route);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRoute),
                new { id = route.RouteID },
                route);
        }

        // PUT: api/Routes/{id}
        // Organisers can update routes for their own events only.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(
            int id,
            RaceDay.API.Models.Route updatedRoute)
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
                    message = "Only Organisers can update routes."
                });
            }

            var existingRoute = await _context.Routes
                .FirstOrDefaultAsync(r => r.RouteID == id);

            if (existingRoute == null)
            {
                return NotFound(new
                {
                    message = "Route not found."
                });
            }

            // Check ownership of the existing route.
            bool ownsExistingEvent = await IsEventOwnedByOrganiser(
                existingRoute.EventID,
                userID.Value);

            if (!ownsExistingEvent)
            {
                return StatusCode(403, new
                {
                    message = "You can only update routes belonging to your own events."
                });
            }

            // Check ownership of the event being assigned to the route.
            bool ownsUpdatedEvent = await IsEventOwnedByOrganiser(
                updatedRoute.EventID,
                userID.Value);

            if (!ownsUpdatedEvent)
            {
                return StatusCode(403, new
                {
                    message = "You can only assign routes to your own events."
                });
            }

            existingRoute.EventID = updatedRoute.EventID;
            existingRoute.RouteName = updatedRoute.RouteName;
            existingRoute.DistanceKM = updatedRoute.DistanceKM;
            existingRoute.ElevationGain = updatedRoute.ElevationGain;
            existingRoute.Description = updatedRoute.Description;
            existingRoute.MapURL = updatedRoute.MapURL;

            await _context.SaveChangesAsync();

            return Ok(existingRoute);
        }

        // DELETE: api/Routes/{id}
        // Organisers can delete routes for their own events only.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
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
                    message = "Only Organisers can delete routes."
                });
            }

            var route = await _context.Routes
                .FirstOrDefaultAsync(r => r.RouteID == id);

            if (route == null)
            {
                return NotFound(new
                {
                    message = "Route not found."
                });
            }

            bool ownsEvent = await IsEventOwnedByOrganiser(
                route.EventID,
                userID.Value);

            if (!ownsEvent)
            {
                return StatusCode(403, new
                {
                    message = "You can only delete routes belonging to your own events."
                });
            }

            _context.Routes.Remove(route);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Route deleted successfully."
            });
        }
    }
}