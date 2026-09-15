namespace SchoolManagementSystem.Data.Entities
{
    public class Student
    {
        public int StudentId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int ClassGroupId { get; set; }
        public ClassGroup ClassGroup { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
