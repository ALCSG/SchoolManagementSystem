using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;

namespace SchoolManagementSystem.Data.Repository
{
    public class SubjectRepository : Repository<Subject>, ISubjectRepository
    {
        public SubjectRepository(DataContext context) : base(context) 
        {
            
        }

        public async Task<IEnumerable<Subject>> GetAllWithDetailsAsync()
                => await _context.Subjects
                    .Include(s => s.Courses)
                    .Include(s => s.Enrollments)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

        public async Task<Subject?> GetByIdWithDetailsAsync(int id)
                => await _context.Subjects
                    .Include(s => s.Courses)
                    .Include(s => s.Enrollments)
                    .FirstOrDefaultAsync(s => s.SubjectId == id);

        public async Task<bool> NameExistsAsync(string name, int? excludeSubjectId = null)
                => await _context.Subjects.AnyAsync(s => s.Name == name && (excludeSubjectId == null || s.SubjectId != excludeSubjectId));

        public async Task<List<Subject>> GetByIdsAsync(IEnumerable<int> ids)
                => await _context.Subjects.Where(s => ids.Contains(s.SubjectId)).ToListAsync();
    }
}
