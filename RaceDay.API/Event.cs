using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Events")]
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        public int OrganiserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0.1, 999.99)]
        public decimal DistanceKM { get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression("^(Run|Walk|Cycle)$",
            ErrorMessage = "Event type must be Run, Walk, or Cycle.")]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        // Link to the banner picture stored in Azure Blob Storage.
        [MaxLength(500)]
        public string? BannerImageUrl { get; set; }

        [ForeignKey(nameof(OrganiserID))]
        public RaceDayUser? Organiser { get; set; }
    }
}