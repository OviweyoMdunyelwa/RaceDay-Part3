using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Enrolments")]
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }

        // Set by the API from the logged-in user, so the website does not send it.
        public int ParticipantID { get; set; }

        public DateTime EnrolmentDate { get; set; }

        // Set by the API when the enrolment is created.
        [MaxLength(20)]
        public string? RaceNumber { get; set; }

        // Set by the API when the enrolment is created.
        [MaxLength(20)]
        public string? PaymentStatus { get; set; }

        [ForeignKey(nameof(EventID))]
        public Event? Event { get; set; }

        [ForeignKey(nameof(CategoryID))]
        public Category? Category { get; set; }

        [ForeignKey(nameof(ParticipantID))]
        public RaceDayUser? Participant { get; set; }
    }
}