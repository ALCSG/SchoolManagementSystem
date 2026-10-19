using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Data.Interface
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<IEnumerable<Course>> GetAllWithDetailsAsync();
        Task<Course?> GetByIdWithDetailsAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeCourseId = null);
    }
}
