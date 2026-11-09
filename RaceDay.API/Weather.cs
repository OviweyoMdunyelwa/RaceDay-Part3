
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Weather")]
    public class Weather
    {
        [Key]
        public int WeatherID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        [Required]
        public DateTime ForecastDate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(typeof(decimal), "-100", "100")]
        public decimal Temperature { get; set; }

        [Required]
        [MaxLength(100)]
        public string Conditions { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        [Range(typeof(decimal), "0", "999.99")]
        public decimal WindSpeed { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(typeof(decimal), "0", "100")]
        public decimal Humidity { get; set; }

        [ForeignKey(nameof(EventID))]
        public Event? Event { get; set; }
    }
}
