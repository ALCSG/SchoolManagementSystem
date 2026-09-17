using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Data.Entities
{
    public class Alert
    {
        public int AlertId { get; set; }

        [Required, MaxLength(500)]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsResolved { get; set; } = false;

        [ForeignKey(nameof(ReportedBy))]
        public string ReportedByUserId { get; set; }

        public AppUser ReportedBy { get; set; }
    }
}
