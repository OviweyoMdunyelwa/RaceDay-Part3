
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Results")]
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public TimeSpan? ChipTime { get; set; }

        [Range(1, int.MaxValue)]
        public int? PositionOverall { get; set; }

        [Range(1, int.MaxValue)]
        public int? PositionCategory { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(typeof(decimal), "0", "999.99")]
        public decimal? Pace { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        [ForeignKey("EnrolmentID")]
        public Enrolment? Enrolment { get; set; }
    }
}
