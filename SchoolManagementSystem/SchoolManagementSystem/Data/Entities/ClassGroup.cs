namespace SchoolManagementSystem.Data.Entities
{
    public class ClassGroup
    {
        public int ClassGroupId { get; set; }

        public string Description { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }

        public ICollection<Student> Students { get; set; }
    }
}
