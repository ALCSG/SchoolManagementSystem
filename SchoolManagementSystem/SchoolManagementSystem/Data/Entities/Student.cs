namespace SchoolManagementSystem.Data.Entities
{
    public class Student
    {
        public int StudentId { get; set; }

        public string StudentPhotoPath { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public int ClassGroupId { get; set; }
        public ClassGroup ClassGroup { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
