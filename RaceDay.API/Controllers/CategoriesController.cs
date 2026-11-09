using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        // Public category browsing.
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        // GET: api/Categories/1
        // Public category details.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            return Ok(category);
        }

        // POST: api/Categories
        // Organisers can create categories for their own events.
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
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
                    message = "Only Organisers can create categories."
                });
            }

            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                return BadRequest(new
                {
                    message = "Category name is required."
                });
            }

            bool eventOwnedByOrganiser = await _context.Events
                .AnyAsync(e =>
                    e.EventID == category.EventID &&
                    e.OrganiserID == userID.Value);

            if (!eventOwnedByOrganiser)
            {
                return BadRequest(new
                {
                    message = "The event does not exist or does not belong to you."
                });
            }

            category.CategoryName = category.CategoryName.Trim();

            // Prevent EF Core from inserting a nested Event.
            category.Event = null;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryID },
                category);
        }

        // PUT: api/Categories/1
        // Organisers can update categories belonging to their own events.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            Category updatedCategory)
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
                    message = "Only Organisers can update categories."
                });
            }

            if (string.IsNullOrWhiteSpace(updatedCategory.CategoryName))
            {
                return BadRequest(new
                {
                    message = "Category name is required."
                });
            }

            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (existingCategory == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            bool ownsCurrentEvent = await _context.Events
                .AnyAsync(e =>
                    e.EventID == existingCategory.EventID &&
                    e.OrganiserID == userID.Value);

            if (!ownsCurrentEvent)
            {
                return StatusCode(403, new
                {
                    message = "You cannot update a category belonging to another Organiser."
                });
            }

            bool ownsTargetEvent = await _context.Events
                .AnyAsync(e =>
                    e.EventID == updatedCategory.EventID &&
                    e.OrganiserID == userID.Value);

            if (!ownsTargetEvent)
            {
                return BadRequest(new
                {
                    message = "The target event does not exist or does not belong to you."
                });
            }

            existingCategory.EventID = updatedCategory.EventID;
            existingCategory.CategoryName = updatedCategory.CategoryName.Trim();
            existingCategory.DistanceKM = updatedCategory.DistanceKM;
            existingCategory.EntryFee = updatedCategory.EntryFee;
            existingCategory.AgeGroup = updatedCategory.AgeGroup;
            existingCategory.Description = updatedCategory.Description;

            await _context.SaveChangesAsync();

            return Ok(existingCategory);
        }

        // DELETE: api/Categories/1
        // Organisers can delete categories belonging to their own events.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
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
                    message = "Only Organisers can delete categories."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            bool ownsEvent = await _context.Events
                .AnyAsync(e =>
                    e.EventID == category.EventID &&
                    e.OrganiserID == userID.Value);

            if (!ownsEvent)
            {
                return StatusCode(403, new
                {
                    message = "You cannot delete a category belonging to another Organiser."
                });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }
    }
}