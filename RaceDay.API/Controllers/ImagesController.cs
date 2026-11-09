using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly BlobStorageService _blobs;

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        public ImagesController(RaceDayDbContext context, BlobStorageService blobs)
        {
            _context = context;
            _blobs = blobs;
        }

        // Returns a message if the file is not acceptable, otherwise null.
        private static string? CheckImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return "Please choose an image file.";

            if (file.Length > MaxBytes)
                return "The image must be 5 MB or smaller.";

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension) ||
                !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return "Only JPG, PNG, GIF or WEBP images are allowed.";
            }

            return null;
        }

        // POST: api/events/5/banner
        // Organiser only, and only for their own event.
        [HttpPost("api/events/{id:int}/banner")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile? file)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null || role == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new { message = "Only Organisers can upload event banners." });
            }

            var eventItem = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (eventItem.OrganiserID != userID.Value)
            {
                return StatusCode(403, new { message = "You can only change banners of your own events." });
            }

            string? problem = CheckImage(file);
            if (problem != null)
            {
                return BadRequest(new { message = problem });
            }

            string url;
            try
            {
                url = await _blobs.UploadAsync(file!, "event-banners");
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "The image could not be stored. Please check the Azure storage settings."
                });
            }

            eventItem.BannerImageUrl = url;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Banner uploaded.", imageUrl = url });
        }

        // POST: api/users/profile/image
        // Any logged-in user can change their own profile picture.
        [HttpPost("api/users/profile/image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile? file)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            var user = await _context.RaceDayUsers.FirstOrDefaultAsync(u => u.UserID == userID.Value);

            if (user == null)
            {
                return NotFound(new { message = "User profile not found." });
            }

            string? problem = CheckImage(file);
            if (problem != null)
            {
                return BadRequest(new { message = problem });
            }

            string url;
            try
            {
                url = await _blobs.UploadAsync(file!, "profile-images");
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "The image could not be stored. Please check the Azure storage settings."
                });
            }

            user.ProfileImageUrl = url;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile picture uploaded.", imageUrl = url });
        }

        // GET: api/media/event-banners/abc.jpg
        // Hands a stored picture to the website (the Azure container stays private).
        [HttpGet("api/media/{*path}")]
        public async Task<IActionResult> GetImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path) ||
                path.Contains("..") ||
                !(path.StartsWith("event-banners/") || path.StartsWith("profile-images/")))
            {
                return NotFound();
            }

            try
            {
                var image = await _blobs.DownloadAsync(path);

                if (image == null)
                {
                    return NotFound();
                }

                return File(image.Value.Stream, image.Value.ContentType);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}