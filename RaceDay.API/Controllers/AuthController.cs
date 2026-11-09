using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<RaceDayUser> _passwordHasher;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<RaceDayUser>();
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "First name, last name, email and password are required."
                });
            }

            if (!request.Email.Contains('@') ||
                !request.Email.Contains('.'))
            {
                return BadRequest(new
                {
                    message = "Please provide a valid email address."
                });
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    message = "Password must be at least 8 characters long."
                });
            }

            string role = request.Role?.Trim() ?? string.Empty;

            if (!role.Equals("Organiser", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Role must be Organiser or Participant."
                });
            }

            role = role.Equals("Organiser", StringComparison.OrdinalIgnoreCase)
                ? "Organiser"
                : "Participant";

            string email = request.Email.Trim();

            bool emailExists = await _context.RaceDayUsers
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "An account with this email already exists."
                });
            }

            var user = new RaceDayUser
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Role = role,
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            _context.RaceDayUsers.Add(user);
            await _context.SaveChangesAsync();

            return Created("", new
            {
                message = "Registration successful.",
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            });
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Email and password are required."
                });
            }

            string email = request.Email.Trim();

            var user = await _context.RaceDayUsers
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);

            return Ok(new
            {
                message = "Login successful.",
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            });
        }

        // LOGOUT
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Ok(new
            {
                message = "Logout successful."
            });
        }
    }
}