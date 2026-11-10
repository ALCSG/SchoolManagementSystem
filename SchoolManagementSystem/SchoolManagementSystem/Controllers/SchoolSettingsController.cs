using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class SchoolSettingsController : Controller
    {
        private readonly ISchoolSettingsRepository _settingsRepository;

        public SchoolSettingsController(ISchoolSettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _settingsRepository.GetSettingsAsync();

            return View(settings);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var settings = await _settingsRepository.GetSettingsAsync();

            if (settings == null)
                return NotFound();

            var model = new SchoolSettingsViewModel
            {
                MaxAbsencePercentage = settings.MaxAbsencePercentage
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SchoolSettingsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var settings = await _settingsRepository.GetSettingsAsync();

            if (settings == null)
                return NotFound();

            settings.MaxAbsencePercentage = model.MaxAbsencePercentage;

            await _settingsRepository.SaveChangesAsync();

            TempData["Message"] = "School settings updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
