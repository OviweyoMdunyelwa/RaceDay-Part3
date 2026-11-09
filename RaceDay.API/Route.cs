
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Routes")]
    public class Route
    {
        [Key]
        public int RouteID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        [Required]
        [MaxLength(100)]
        public string RouteName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        [Range(typeof(decimal), "0.1", "999.99")]
        public decimal DistanceKM { get; set; }

        [Column(TypeName = "decimal(7,2)")]
        [Range(typeof(decimal), "0", "99999.99")]
        public decimal ElevationGain { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        [Url]
        public string? MapURL { get; set; }

        [ForeignKey(nameof(EventID))]
        public Event? Event { get; set; }
    }
}
