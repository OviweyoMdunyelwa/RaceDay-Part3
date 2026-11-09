
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public WeatherController(RaceDayDbContext context)
        {
            _context = context;
        }

        private (int? UserID, string? Role) GetSessionUser()
        {
            return (
                HttpContext.Session.GetInt32("UserID"),
                HttpContext.Session.GetString("Role")
            );
        }

        private async Task<bool> OwnsEvent(int eventID, int userID)
        {
            return await _context.Events.AnyAsync(e =>
                e.EventID == eventID &&
                e.OrganiserID == userID);
        }

        // GET: api/Weather
        [HttpGet]
        public async Task<IActionResult> GetWeather()
        {
            var (userID, role) = GetSessionUser();

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            var weatherQuery = _context.Weather.AsQueryable();

            if (role == "Organiser")
            {
                weatherQuery = weatherQuery.Where(w =>
                    _context.Events.Any(e =>
                        e.EventID == w.EventID &&
                        e.OrganiserID == userID.Value));
            }
            else if (role == "Participant")
            {
                // Participants can view weather for events they entered.
                weatherQuery = weatherQuery.Where(w =>
                    _context.Enrolments.Any(en =>
                        en.EventID == w.EventID &&
                        en.ParticipantID == userID.Value));
            }
            else
            {
                return StatusCode(403, new { message = "Invalid role." });
            }

            return Ok(await weatherQuery.ToListAsync());
        }

        // GET: api/Weather/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWeatherById(int id)
        {
            var (userID, role) = GetSessionUser();

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            var weather = await _context.Weather
                .FirstOrDefaultAsync(w => w.WeatherID == id);

            if (weather == null)
            {
                return NotFound(new { message = "Weather record not found." });
            }

            bool allowed = role switch
            {
                "Organiser" => await OwnsEvent(weather.EventID, userID.Value),
                "Participant" => await _context.Enrolments.AnyAsync(en =>
                    en.EventID == weather.EventID &&
                    en.ParticipantID == userID.Value),
                _ => false
            };

            if (!allowed)
            {
                return StatusCode(403, new
                {
                    message = "You cannot view weather for this event."
                });
            }

            return Ok(weather);
        }

        // POST: api/Weather
        [HttpPost]
        public async Task<IActionResult> CreateWeather(Weather weather)
        {
            var (userID, role) = GetSessionUser();

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can create weather records."
                });
            }

            if (weather.ForecastDate == default ||
                string.IsNullOrWhiteSpace(weather.Conditions) ||
                weather.Temperature < -100 || weather.Temperature > 100 ||
                weather.WindSpeed < 0 ||
                weather.Humidity < 0 || weather.Humidity > 100)
            {
                return BadRequest(new
                {
                    message = "Please provide valid weather details."
                });
            }

            if (!await OwnsEvent(weather.EventID, userID.Value))
            {
                return StatusCode(403, new
                {
                    message = "You can only add weather to your own events."
                });
            }

            _context.Weather.Add(weather);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetWeatherById),
                new { id = weather.WeatherID },
                weather);
        }

        // PUT: api/Weather/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWeather(
            int id,
            Weather updatedWeather)
        {
            var (userID, role) = GetSessionUser();

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can update weather records."
                });
            }

            var existingWeather = await _context.Weather
                .FirstOrDefaultAsync(w => w.WeatherID == id);

            if (existingWeather == null)
            {
                return NotFound(new { message = "Weather record not found." });
            }

            if (!await OwnsEvent(existingWeather.EventID, userID.Value))
            {
                return StatusCode(403, new
                {
                    message = "You can only update weather for your own events."
                });
            }

            if (updatedWeather.ForecastDate == default ||
                string.IsNullOrWhiteSpace(updatedWeather.Conditions) ||
                updatedWeather.Temperature < -100 ||
                updatedWeather.Temperature > 100 ||
                updatedWeather.WindSpeed < 0 ||
                updatedWeather.Humidity < 0 ||
                updatedWeather.Humidity > 100)
            {
                return BadRequest(new
                {
                    message = "Please provide valid weather details."
                });
            }

            // Keep the record attached to its original event.
            existingWeather.ForecastDate = updatedWeather.ForecastDate;
            existingWeather.Temperature = updatedWeather.Temperature;
            existingWeather.Conditions = updatedWeather.Conditions.Trim();
            existingWeather.WindSpeed = updatedWeather.WindSpeed;
            existingWeather.Humidity = updatedWeather.Humidity;

            await _context.SaveChangesAsync();

            return Ok(existingWeather);
        }

        // DELETE: api/Weather/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWeather(int id)
        {
            var (userID, role) = GetSessionUser();

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can delete weather records."
                });
            }

            var weather = await _context.Weather
                .FirstOrDefaultAsync(w => w.WeatherID == id);

            if (weather == null)
            {
                return NotFound(new { message = "Weather record not found." });
            }

            if (!await OwnsEvent(weather.EventID, userID.Value))
            {
                return StatusCode(403, new
                {
                    message = "You can only delete weather for your own events."
                });
            }

            _context.Weather.Remove(weather);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Weather record deleted successfully." });
        }
    }
}
