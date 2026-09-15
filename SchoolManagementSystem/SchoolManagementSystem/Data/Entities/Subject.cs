namespace SchoolManagementSystem.Data.Entities
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string Name { get; set; }

        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
