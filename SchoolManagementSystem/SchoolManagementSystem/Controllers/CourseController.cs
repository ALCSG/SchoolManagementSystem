using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ISubjectRepository _subjectRepository;

        public CourseController(ICourseRepository courseRepository, ISubjectRepository subjectRepository)
        {
            _courseRepository = courseRepository;
            _subjectRepository = subjectRepository;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _courseRepository.GetAllWithDetailsAsync();
            return View(courses);
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseRepository.GetByIdWithDetailsAsync(id);
            if (course == null)
                return NotFound();

            return View(course);
        }

        public async Task<IActionResult> Create()
        {
            var model = new CourseViewModel
            {
                Subjects = await GetSubjectsSelectListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            if (await _courseRepository.NameExistsAsync(model.Name))
                ModelState.AddModelError(nameof(model.Name), "A course with this name already exists");

            if (!ModelState.IsValid)
            {
                model.Subjects = await GetSubjectsSelectListAsync();
                return View(model);
            }

            var course = new Course
            {
                Name = model.Name,
                Subjects = await _subjectRepository.GetByIdsAsync(model.SelectedSubjectIds)
            };

            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseRepository.GetByIdWithDetailsAsync(id);
            if (course == null)
                return NotFound();

            var model = new CourseViewModel
            {
                CourseId = course.CourseId,
                Name = course.Name,
                SelectedSubjectIds = course.Subjects.Select(s => s.SubjectId).ToList(),
                Subjects = await GetSubjectsSelectListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseViewModel model)
        {
            if (await _courseRepository.NameExistsAsync(model.Name, model.CourseId))
                ModelState.AddModelError(nameof(model.Name), "A course with this name already exists");

            if (!ModelState.IsValid)
            {
                model.Subjects = await GetSubjectsSelectListAsync();
                return View(model);
            }

            var course = await _courseRepository.GetByIdWithDetailsAsync(model.CourseId);

            if (course == null)
                return NotFound();

            course.Name = model.Name;

            var selectedSubjects = await _subjectRepository.GetByIdsAsync(model.SelectedSubjectIds);
            course.Subjects.Clear();
            foreach (var subject in selectedSubjects)
                course.Subjects.Add(subject);

            await _courseRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetByIdWithDetailsAsync(id);
            if (course == null)
                return NotFound();

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _courseRepository.GetByIdWithDetailsAsync(id);
            if (course == null)
                return NotFound();

            if (course.Groups.Any() || course.Subjects.Any())
            {
                ModelState.AddModelError(string.Empty,
                    "This course cannot be deleted because it has class groups or linked subjects. Remove those first.");
                return View("Delete", course);
            }

            try
            {
                _courseRepository.Delete(course);
                await _courseRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty, "This course could not be deleted because it is still in use.");
                return View("Delete", course);
            }
        }

        private async Task<IEnumerable<SelectListItem>> GetSubjectsSelectListAsync()
        {
            var subjects = await _subjectRepository.GetAllAsync();
            return subjects
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.SubjectId.ToString(), Text = s.Name });
        }
    }
}
