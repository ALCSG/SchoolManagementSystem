using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Data.Interface
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<IEnumerable<Subject>> GetAvailableSubjectsForStudentAsync(int studentId);

        Task<Enrollment?> GetByIdWithDetailsAsync(int id);

        Task<bool> EnrollmentExistsAsync(int studentId, int? subjectId);
    }
}