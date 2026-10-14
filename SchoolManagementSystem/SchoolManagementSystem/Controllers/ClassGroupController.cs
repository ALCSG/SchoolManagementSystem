using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Data.Entities;
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
                ModelState.AddModelError(nameof(model.Description), "A class group with this description already exists");

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
    }
}