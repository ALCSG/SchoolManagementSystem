using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Helpers;

namespace SchoolManagementSystem.Controllers
{
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Employee)]
    public class SubjectController : Controller
    {
        private readonly ISubjectRepository _subjectRepository;

        public SubjectController(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<IActionResult> Index()
        {
            var subjects = await _subjectRepository.GetAllWithDetailsAsync();
            return View(subjects);
        }

        public async Task<IActionResult> Details(int id)
        {
            var subject = await _subjectRepository.GetByIdWithDetailsAsync(id);

            if (subject == null)
                return NotFound();

            return View(subject);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,TotalHours")] Subject subject)
        {
            if (await _subjectRepository.NameExistsAsync(subject.Name))
                ModelState.AddModelError(nameof(subject.Name), "A subject with this name already exists");

            if (subject.TotalHours != 25 && subject.TotalHours != 50)
                ModelState.AddModelError(nameof(subject.TotalHours), "Total hours for the subject must be either 25 or 50.");

            if (!ModelState.IsValid)
                return View(subject);

            await _subjectRepository.AddAsync(subject);
            await _subjectRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);

            if (subject == null)
                return NotFound();

            return View(subject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("SubjectId,Name,TotalHours")] Subject model)
        {
            if (await _subjectRepository.NameExistsAsync(model.Name, model.SubjectId))
                ModelState.AddModelError(nameof(model.Name),"A subject with this name already exists.");

            if (model.TotalHours != 25 && model.TotalHours != 50)
                ModelState.AddModelError(nameof(model.TotalHours),"Total hours for the subject must be either 25 or 50.");

            if (!ModelState.IsValid)
                return View(model);

            var subject = await _subjectRepository.GetByIdAsync(model.SubjectId);

            if (subject == null)
                return NotFound();

            subject.Name = model.Name;
            subject.TotalHours = model.TotalHours;

            _subjectRepository.Update(subject);
            await _subjectRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _subjectRepository.GetByIdWithDetailsAsync(id);

            if (subject == null)
                return NotFound();

            return View(subject);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subject = await _subjectRepository.GetByIdWithDetailsAsync(id);

            if (subject == null)
                return NotFound();

            if (subject.Courses.Any() || subject.Enrollments.Any())
            {
                ModelState.AddModelError(string.Empty, "This subject cannot be deleted because it is used by courses or has enrollments. Remove those links first.");
                return View("Delete", subject);
            }

            try
            {
                _subjectRepository.Delete(subject);
                await _subjectRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty, "This subject could not be deleted because it is still in use.");
                return View("Delete", subject);
            }
        }
    }
}
