using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Data.Interface
{
    public interface IAlertRepository : IRepository<Alert>
    {
        Task<IEnumerable<Alert>> GetOpenAlertsWithReporterAsync();
        Task<IEnumerable<Alert>> GetClosedAlertsWithReporterAsync();
    }
}
