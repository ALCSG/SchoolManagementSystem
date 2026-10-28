using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models;
using System.Security.Claims;

namespace SchoolManagementSystem.Controllers
{
    public class AlertController : Controller
    {
        private readonly IAlertRepository _alertRepository;

        public AlertController(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }

        [Authorize(Roles = AppRoles.Employee)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.Employee)]
        public IActionResult Create()
        {
            var model = new AlertViewModel
            {
                Message = string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlertViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to identify the current user.");
                return View(model);
            }

            try
            {
                var alert = new Alert
                {
                    Title = model.Title,
                    Message = model.Message,
                    ReportedByUserId = user
                };


                await _alertRepository.AddAsync(alert);
                await _alertRepository.SaveChangesAsync();

                TempData["Message"] = "Your alert was sent to the administrator.";

                return RedirectToAction(nameof(Create));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "The alert could not be created. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int alertId)
        {
            var alert = await _alertRepository.GetByIdAsync(alertId);

            if(alert == null)
                return NotFound();

            alert.IsResolved = true;

            await _alertRepository.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unresolve(int alertId)
        {
            var alert = await _alertRepository.GetByIdAsync(alertId);

            if(alert == null)
                return NotFound();

            alert.IsResolved = false;

            await _alertRepository.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Details(int id)
        {
            var alert = await _alertRepository.GetByIdWithDetailsAsync(id);

            if (alert == null)
                return NotFound();

            return View(alert);
        }
    }
}
