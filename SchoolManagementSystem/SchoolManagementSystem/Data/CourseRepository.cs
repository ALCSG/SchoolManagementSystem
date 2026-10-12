using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Data
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
    }
}
