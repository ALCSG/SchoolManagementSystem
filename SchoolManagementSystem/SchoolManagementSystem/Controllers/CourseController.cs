using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Employee)]
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Catalog()
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PublicDetails(int id)
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
                Subjects = await _subjectRepository.GetByIdsAsync(model.SelectedSubjectIds),
                Description = model.Description
            };

            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = AppRoles.Admin)]
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
                Description = course.Description,
                Subjects = await GetSubjectsSelectListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseViewModel model)
        {
            if (await _courseRepository.NameExistsAsync(model.Name,model.CourseId))
                ModelState.AddModelError(nameof(model.Name), "A course with this name already exists.");

            if (!ModelState.IsValid)
            {
                model.Subjects = await GetSubjectsSelectListAsync();
                return View(model);
            }

            var course = await _courseRepository
                .GetByIdWithDetailsAsync(model.CourseId);

            if (course == null)
                return NotFound();

            var selectedSubjectIds = model.SelectedSubjectIds
                .Distinct()
                .ToList();

            var removedSubjectIds = course.Subjects
                .Select(subject => subject.SubjectId)
                .Except(selectedSubjectIds)
                .ToList();

            var enrolledSubjectNames = await _courseRepository
                .GetEnrolledSubjectNamesAsync(course.CourseId,removedSubjectIds);

            if (enrolledSubjectNames.Count > 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The following subjects cannot be removed because students in this course still have enrollments in them: " +
                    $"{string.Join(", ", enrolledSubjectNames)}. " +
                    "Please coordinate with the employee responsible for the student records.");
            }

            var selectedSubjects = await _subjectRepository
                .GetByIdsAsync(selectedSubjectIds);

            if (selectedSubjects.Count() != selectedSubjectIds.Count)
                ModelState.AddModelError(string.Empty,"One or more selected subjects no longer exist. Please review your selection.");

            if (!ModelState.IsValid)
            {
                model.Subjects = await GetSubjectsSelectListAsync();
                return View(model);
            }

            course.Name = model.Name;
            course.Subjects.Clear();
            course.Description = model.Description;

            foreach (var subject in selectedSubjects)
                course.Subjects.Add(subject);

            try
            {
                await _courseRepository.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,"The course could not be updated. Please reload the page and try again.");

                model.Subjects = await GetSubjectsSelectListAsync();

                return View(model);
            }

            TempData["Message"] = "Course updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetByIdWithDetailsAsync(id);
            if (course == null)
                return NotFound();

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = AppRoles.Admin)]
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
