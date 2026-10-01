using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Data
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<IEnumerable<Student>> GetAllWithDetailsAsync();
        Task<Student?> GetByIdWithDetailsAsync(int id);
    }

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
    }
}
