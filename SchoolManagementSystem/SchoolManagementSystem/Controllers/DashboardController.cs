using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class DashboardController : Controller
    {
        private readonly IAlertRepository _alertRepository;

        public DashboardController(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                OpenAlerts = await _alertRepository.GetOpenAlertsWithReporterAsync()
            };

            return View(model);
        }
    }
}
