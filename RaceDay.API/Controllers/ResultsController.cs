using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Checks whether an enrolment belongs to an event
        // owned by the currently logged-in organiser.
        private async Task<bool> IsEnrolmentOwnedByOrganiser(
            int enrolmentID,
            int organiserID)
        {
            return await _context.Enrolments.AnyAsync(e =>
                e.EnrolmentID == enrolmentID &&
                _context.Events.Any(ev =>
                    ev.EventID == e.EventID &&
                    ev.OrganiserID == organiserID));
        }

        // GET: api/Results
        // Organisers view results for their own events.
        // Participants view only their own results.
        [HttpGet]
        public async Task<IActionResult> GetResults()
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
                var organiserResults = await _context.Results
                    .Where(r => _context.Enrolments.Any(e =>
                        e.EnrolmentID == r.EnrolmentID &&
                        _context.Events.Any(ev =>
                            ev.EventID == e.EventID &&
                            ev.OrganiserID == userID.Value)))
                    .ToListAsync();

                return Ok(organiserResults);
            }

            if (role == "Participant")
            {
                var participantResults = await _context.Results
                    .Where(r => _context.Enrolments.Any(e =>
                        e.EnrolmentID == r.EnrolmentID &&
                        e.ParticipantID == userID.Value))
                    .ToListAsync();

                return Ok(participantResults);
            }

            return StatusCode(403, new
            {
                message = "You do not have permission to view results."
            });
        }

        // GET: api/Results/1
        // Users can only view results they are authorised to access.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(int id)
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

            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            if (role == "Participant")
            {
                bool belongsToParticipant = await _context.Enrolments
                    .AnyAsync(e =>
                        e.EnrolmentID == result.EnrolmentID &&
                        e.ParticipantID == userID.Value);

                if (!belongsToParticipant)
                {
                    return StatusCode(403, new
                    {
                        message = "You can only view your own results."
                    });
                }
            }
            else if (role == "Organiser")
            {
                bool belongsToOrganiser =
                    await IsEnrolmentOwnedByOrganiser(
                        result.EnrolmentID,
                        userID.Value);

                if (!belongsToOrganiser)
                {
                    return StatusCode(403, new
                    {
                        message = "You can only view results for your own events."
                    });
                }
            }
            else
            {
                return StatusCode(403, new
                {
                    message = "You do not have permission to view results."
                });
            }

            return Ok(result);
        }

        // POST: api/Results
        // Organisers can create results for their own events only.
        [HttpPost]
        public async Task<IActionResult> CreateResult(Result result)
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
                    message = "Only Organisers can create results."
                });
            }

            bool enrolmentExists = await _context.Enrolments
                .AnyAsync(e => e.EnrolmentID == result.EnrolmentID);

            if (!enrolmentExists)
            {
                return BadRequest(new
                {
                    message = "The specified enrolment does not exist."
                });
            }

            bool belongsToOrganiser =
                await IsEnrolmentOwnedByOrganiser(
                    result.EnrolmentID,
                    userID.Value);

            if (!belongsToOrganiser)
            {
                return StatusCode(403, new
                {
                    message = "You can only create results for enrolments in your own events."
                });
            }

            bool resultExists = await _context.Results
                .AnyAsync(r => r.EnrolmentID == result.EnrolmentID);

            if (resultExists)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });
            }

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultID },
                result);
        }

        // PUT: api/Results/1
        // Organisers can update results for their own events only.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(
            int id,
            Result updatedResult)
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
                    message = "Only Organisers can update results."
                });
            }

            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (existingResult == null)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            // Check ownership of the existing result.
            bool ownsExistingResult =
                await IsEnrolmentOwnedByOrganiser(
                    existingResult.EnrolmentID,
                    userID.Value);

            if (!ownsExistingResult)
            {
                return StatusCode(403, new
                {
                    message = "You can only update results for your own events."
                });
            }

            bool enrolmentExists = await _context.Enrolments
                .AnyAsync(e => e.EnrolmentID == updatedResult.EnrolmentID);

            if (!enrolmentExists)
            {
                return BadRequest(new
                {
                    message = "The specified enrolment does not exist."
                });
            }

            // Also check ownership of the new enrolment.
            bool ownsUpdatedEnrolment =
                await IsEnrolmentOwnedByOrganiser(
                    updatedResult.EnrolmentID,
                    userID.Value);

            if (!ownsUpdatedEnrolment)
            {
                return StatusCode(403, new
                {
                    message = "You can only assign results to enrolments in your own events."
                });
            }

            bool duplicateResult = await _context.Results
                .AnyAsync(r =>
                    r.EnrolmentID == updatedResult.EnrolmentID &&
                    r.ResultID != id);

            if (duplicateResult)
            {
                return Conflict(new
                {
                    message = "Another result already exists for this enrolment."
                });
            }

            existingResult.EnrolmentID = updatedResult.EnrolmentID;
            existingResult.FinishTime = updatedResult.FinishTime;
            existingResult.ChipTime = updatedResult.ChipTime;
            existingResult.PositionOverall = updatedResult.PositionOverall;
            existingResult.PositionCategory = updatedResult.PositionCategory;
            existingResult.Pace = updatedResult.Pace;
            existingResult.Status = updatedResult.Status;

            await _context.SaveChangesAsync();

            return Ok(existingResult);
        }

        // DELETE: api/Results/1
        // Organisers can delete results for their own events only.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
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
                    message = "Only Organisers can delete results."
                });
            }

            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            bool belongsToOrganiser =
                await IsEnrolmentOwnedByOrganiser(
                    result.EnrolmentID,
                    userID.Value);

            if (!belongsToOrganiser)
            {
                return StatusCode(403, new
                {
                    message = "You can only delete results for your own events."
                });
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result deleted successfully."
            });
        }
    }
}
