using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    public class ClassGroupController : Controller
    {
        private readonly IClassGroupRepository _classGroupRepository;
        private readonly ICourseRepository _courseRepository;

        public ClassGroupController(IClassGroupRepository classGroupRepository, ICourseRepository courseRepository)
        {
            _classGroupRepository = classGroupRepository;
            _courseRepository = courseRepository;
        }

        private async Task<IEnumerable<SelectListItem>> GetCoursesSelectListAsync()
        {
            var courses = await _courseRepository.GetAllAsync();

            return courses
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.Name
                });
        }

        public async Task<IActionResult> Details(int id)
        {
            var classGroup = await _classGroupRepository.GetByIdWithDetailsAsync(id);

            if (classGroup == null)
                return NotFound();

            return View(classGroup);
        }

        public async Task<IActionResult> Index()
        {
            var classGroup = await _classGroupRepository.GetAllWithDetailsAsync();

            return View(classGroup);
        }

        public async Task<IActionResult> Create()
        {
            var model = new ClassGroupViewModel
            {
                CoursesList = await GetCoursesSelectListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassGroupViewModel model)
        {
            if (await _classGroupRepository.DescriptionExistsInCourseAsync(model.Description, model.CourseId))
                ModelState.AddModelError(nameof(model.Description), "A class group with this description already exists in this course");

            if (!ModelState.IsValid)
            {
                model.CoursesList = await GetCoursesSelectListAsync();
                return View(model);
            }

            var classGroup = new ClassGroup()
            {
                Description = model.Description,
                CourseId = model.CourseId
            };

            await _classGroupRepository.AddAsync(classGroup);
            await _classGroupRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var classGroup = await _classGroupRepository.GetByIdAsync(id);

            if (classGroup == null)
                return NotFound();

            var model = new ClassGroupViewModel()
            {
                ClassGroupId = classGroup.ClassGroupId,
                CourseId = classGroup.CourseId,
                CoursesList = await GetCoursesSelectListAsync(),
                Description = classGroup.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassGroupViewModel model)
        {
            var classGroupEdit = await _classGroupRepository.GetByIdWithDetailsAsync(model.ClassGroupId);

            if (classGroupEdit == null)
                return NotFound();

            if (model.CourseId != classGroupEdit.CourseId && classGroupEdit.Students.Any())
            {
                ModelState.AddModelError(nameof(model.CourseId), "This class group has students enrolled and can't be moved to another course");
            }

            if (await _classGroupRepository.DescriptionExistsInCourseAsync(model.Description, model.CourseId, model.ClassGroupId))
            {
                ModelState.AddModelError(nameof(model.Description), "A class group with this description already exists in this course");
            }

            if (!ModelState.IsValid)
            {
                model.CoursesList = await GetCoursesSelectListAsync();
                return View(model);
            }

            classGroupEdit.Description = model.Description;
            classGroupEdit.CourseId = model.CourseId;

            await _classGroupRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var classGroup = await _classGroupRepository.GetByIdWithDetailsAsync(id);

            if(classGroup == null)
               return NotFound();

            return View(classGroup);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classGroup = await _classGroupRepository.GetByIdWithDetailsAsync(id);

            if (classGroup == null)
                return NotFound();

            if (classGroup.Students.Any())
            {
                ModelState.AddModelError(nameof(classGroup.Students), "The selected class group can't be deleted due to having enrolled students");
                return View(classGroup);
            }

            try
            {
                _classGroupRepository.Delete(classGroup);
                await _classGroupRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty, "This class group could not be deleted because it is still in use.");
                return View("Delete", classGroup);
            }
        }
    }
}