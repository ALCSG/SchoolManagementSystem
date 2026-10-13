using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Data
{
    public class ClassGroupRepository : Repository<ClassGroup>, IClassGroupRepository
    {
        public ClassGroupRepository(DataContext context) : base(context) { }

        public async Task<IEnumerable<ClassGroup>> GetAllWithDetailsAsync()
            => await _context.ClassGroups
                .Include(g => g.Course)
                .Include(g => g.Students)
                .OrderBy(g => g.Course.Name)
                .ThenBy(g => g.Description)
                .ToListAsync();

        public async Task<ClassGroup?> GetByIdWithDetailsAsync(int id)
            => await _context.ClassGroups
                .Include(g => g.Course)
                .Include(g => g.Students)
                .ThenInclude(s => s.AppUser)
                .FirstOrDefaultAsync(g => g.ClassGroupId == id);

        public async Task<bool> DescriptionExistsInCourseAsync(string description, int courseId, int? excludeClassGroupId = null)
            => await _context.ClassGroups
                .AnyAsync(g => g.Description == description && g.CourseId == courseId && (excludeClassGroupId == null || g.ClassGroupId != excludeClassGroupId));
    }
}