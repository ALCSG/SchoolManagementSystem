using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Data.Interface
{
    public interface ISchoolSettingsRepository : IRepository<SchoolSettings>
    {
        Task<SchoolSettings?> GetSettingsAsync();
    }
}
