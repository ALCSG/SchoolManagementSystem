namespace SchoolManagementSystem.Data.Entities
{
    public class Course
    {
        public int CourseId { get; set; }

        public string Name { get; set; }

        public ICollection<ClassGroup> Groups { get; set; } = new List<ClassGroup>();

        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    }
}
