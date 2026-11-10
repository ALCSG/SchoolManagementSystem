using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;

namespace SchoolManagementSystem.Data.Repository
{
    public class SchoolSettingsRepository : Repository<SchoolSettings>, ISchoolSettingsRepository
    {
        public SchoolSettingsRepository(DataContext context) : base(context) { }

        public async Task<SchoolSettings?> GetSettingsAsync()
            => await _context.Settings
                .SingleOrDefaultAsync();
    }
}
