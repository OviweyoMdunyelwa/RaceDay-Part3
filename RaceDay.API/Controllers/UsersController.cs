using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Users/profile
        // View the logged-in user's profile.
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in."
                });
            }

            var user = await _context.RaceDayUsers
                .FirstOrDefaultAsync(u => u.UserID == userID.Value);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User profile not found."
                });
            }

            return Ok(new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.CreatedAt
            });
        }

        // PUT: api/Users/profile
        // Update the logged-in user's profile.
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            UpdateProfileRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in."
                });
            }

            var user = await _context.RaceDayUsers
                .FirstOrDefaultAsync(u => u.UserID == userID.Value);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User profile not found."
                });
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new
                {
                    message = "First name and last name are required."
                });
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.PhoneNumber = request.PhoneNumber?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.Role
            });
        }
    }

    public class UpdateProfileRequest
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}