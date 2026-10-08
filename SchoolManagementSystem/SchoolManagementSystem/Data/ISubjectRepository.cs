using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Data
{
    public interface ISubjectRepository : IRepository<Subject>
    {
        Task<IEnumerable<Subject>> GetAllWithDetailsAsync();

        Task<Subject> GetByIdWithDetailsAsync(int id);

        Task<bool> NameExistsAsync(string name, int? excludeSubjectId = null);
    }
}
