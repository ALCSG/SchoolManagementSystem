using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;

namespace SchoolManagementSystem.Data.Repository
{
    public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(DataContext context) : base(context) { }

        public async Task<bool> EnrollmentExistsAsync(int studentId, int? subjectId)
            => await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.SubjectId == subjectId);

        public async Task<IEnumerable<Subject>> GetAvailableSubjectsForStudentAsync(int studentId)
        {
            var courseId = await _context.Students
                .Where(s => s.StudentId == studentId)
                .Select(s => s.ClassGroup.CourseId)
                .FirstOrDefaultAsync();

            return await _context.Subjects
                .Where(subject => subject.Courses.Any(c => c.CourseId == courseId) && !subject.Enrollments.Any(e => e.StudentId == studentId))
                .OrderBy(subject => subject.Name)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetByIdWithDetailsAsync(int id)
            => await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.AppUser)
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);
    }
}
