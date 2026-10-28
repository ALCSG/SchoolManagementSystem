using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Data.Entities
{
    public class Student
    {
        public int StudentId { get; set; }

        [Required]
        public string StudentPhotoPath { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [ForeignKey(nameof(AppUser))]
        public string? AppUserId { get; set; }

        public AppUser? AppUser { get; set; }

        [ForeignKey(nameof(ClassGroup))]
        public int ClassGroupId { get; set; }

        public ClassGroup ClassGroup { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}