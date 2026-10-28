using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;

namespace SchoolManagementSystem.Data.Repository
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(DataContext context) : base(context) { }

        public async Task<IEnumerable<Student>> GetAllWithDetailsAsync()
            => await _context.Students
                .Include(s => s.AppUser)
                .Include(s => s.ClassGroup)
                .ToListAsync();

        public async Task<Student?> GetByIdWithDetailsAsync(int id)
            => await _context.Students
                .Include(s => s.AppUser)
                .Include(s => s.ClassGroup)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Subject)
                .FirstOrDefaultAsync(s => s.StudentId == id);

        public async Task<IEnumerable<Student>> GetStudentsWithoutAccountAsync()
            => await _context.Students
                .AsNoTracking()
                .Where(s => s.AppUserId == null)
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .ToListAsync();
    }
}
