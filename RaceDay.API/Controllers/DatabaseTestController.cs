
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Data;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseTestController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly ILogger<DatabaseTestController> _logger;

        public DatabaseTestController(
            RaceDayDbContext context,
            ILogger<DatabaseTestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/DatabaseTest
        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                bool connected =
                    await _context.Database.CanConnectAsync();

                if (connected)
                {
                    return Ok(new
                    {
                        message = "Database connection successful."
                    });
                }

                _logger.LogWarning(
                    "The RaceDay database connection check returned false.");

                return StatusCode(503, new
                {
                    message = "The database is currently unavailable."
                });
            }
            catch (Exception ex)
            {
                // Keep technical details in server logs, not API responses.
                _logger.LogError(
                    ex,
                    "An error occurred while checking the database connection.");

                return StatusCode(503, new
                {
                    message = "Unable to verify the database connection."
                });
            }
        }
    }
}
