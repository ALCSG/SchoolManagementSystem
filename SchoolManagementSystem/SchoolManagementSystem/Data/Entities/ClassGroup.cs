using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Data.Entities
{
    public class ClassGroup
    {
        public int ClassGroupId { get; set; }

        [Required, MaxLength(100)]
        public string Description { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }

        public Course Course { get; set; }

        public ICollection<Student> Students { get; set; }
    }
}
