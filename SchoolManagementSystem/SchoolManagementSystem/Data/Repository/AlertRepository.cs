using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;

namespace SchoolManagementSystem.Data.Repository
{
    public class AlertRepository : Repository<Alert>, IAlertRepository
    {

        public AlertRepository(DataContext context) : base(context) { }

        public async Task<IEnumerable<Alert>> GetOpenAlertsWithReporterAsync()
                    => await _context.Alerts
                        .Include(a => a.ReportedBy)
                        .Where(a => !a.IsResolved)
                        .OrderByDescending(a => a.CreatedAt)
                        .ToListAsync();

        public async Task<IEnumerable<Alert>> GetClosedAlertsWithReporterAsync()
                    => await _context.Alerts
                        .Include(a => a.ReportedBy)
                        .Where(a => a.IsResolved)
                        .OrderByDescending(a => a.CreatedAt)
                        .ToListAsync();

    }
}
