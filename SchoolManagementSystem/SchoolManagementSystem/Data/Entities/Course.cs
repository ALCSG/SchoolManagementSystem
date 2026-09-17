using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Data.Entities
{
    public class Course
    {
        public int CourseId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public ICollection<ClassGroup> Groups { get; set; } = new List<ClassGroup>();

        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    }
}
