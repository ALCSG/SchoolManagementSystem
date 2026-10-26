using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<Alert> OpenAlerts { get; set; } = new List<Alert>();
    }
}
