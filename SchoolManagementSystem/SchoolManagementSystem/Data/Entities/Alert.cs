using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Data.Entities
{
    public class Alert
    {
        public int AlertId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsResolved { get; set; } = false;

        [ForeignKey(nameof(ReportedBy))]
        public string ReportedByUserId { get; set; } = string.Empty;

        public AppUser ReportedBy { get; set; } = null!;

        public int? StudentId { get; set; }

        public Student? Student { get; set; }
    }
}