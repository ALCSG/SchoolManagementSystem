using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;

namespace SchoolManagementSystem.Data.Repository
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(DataContext context) : base(context) { }

        public async Task<IEnumerable<Course>> GetAllWithDetailsAsync()
            => await _context.Courses
                .Include(c => c.Groups)
                .Include(c => c.Subjects)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public async Task<Course?> GetByIdWithDetailsAsync(int id)
            => await _context.Courses
                .Include(c => c.Groups)
                .Include(c => c.Subjects)
                .FirstOrDefaultAsync(c => c.CourseId == id);

        public async Task<bool> NameExistsAsync(string name, int? excludeCourseId = null)
            => await _context.Courses.AnyAsync(c =>
                c.Name == name && (excludeCourseId == null || c.CourseId != excludeCourseId));

        public async Task<List<string>> GetEnrolledSubjectNamesAsync(int courseId,IEnumerable<int> subjectIds)
        {
            var ids = subjectIds.Distinct().ToList();

            if (ids.Count == 0)
                return new List<string>();

            return await _context.Enrollments
                .Where(enrollment =>
                    enrollment.Student.ClassGroup.CourseId == courseId &&
                    ids.Contains(enrollment.SubjectId))
                .Select(enrollment => enrollment.Subject.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();
        }
    }
}
