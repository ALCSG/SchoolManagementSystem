using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Data.Interface
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<IEnumerable<Student>> GetAllWithDetailsAsync();
        Task<Student?> GetByIdWithDetailsAsync(int id);
    }
}
