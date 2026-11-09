using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.API.Models
{
    [Table("RaceDayUsers")]
    [Index(nameof(Email), IsUnique = true)]
    public class RaceDayUser
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        // Link to the profile picture stored in Azure Blob Storage.
        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}