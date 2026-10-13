using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Data
{
    public interface IClassGroupRepository : IRepository<ClassGroup>
    {
        Task<IEnumerable<ClassGroup>> GetAllWithDetailsAsync();
        Task<ClassGroup?> GetByIdWithDetailsAsync(int id);
        Task<bool> DescriptionExistsInCourseAsync(string description, int courseId, int? excludeClassGroupId = null);
    }
}