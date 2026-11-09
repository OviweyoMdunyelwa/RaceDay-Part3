using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        [Range(0.1, 999.99)]
        public decimal DistanceKM { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal EntryFee { get; set; }

        [MaxLength(50)]
        public string? AgeGroup { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        [ForeignKey(nameof(EventID))]
        public Event? Event { get; set; }
    }
}