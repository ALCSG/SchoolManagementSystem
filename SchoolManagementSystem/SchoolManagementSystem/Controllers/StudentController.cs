using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IClassGroupRepository _classGroupRepository;
        private readonly UserManager<AppUser> _userManager;

        public StudentController(
            IStudentRepository studentRepository,
            IClassGroupRepository classGroupRepository,
            UserManager<AppUser> userManager)
        {
            _studentRepository = studentRepository;
            _classGroupRepository = classGroupRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _studentRepository.GetAllWithDetailsAsync();
            return View(students);
        }

        public async Task<IActionResult> Create()
        {
            var model = new StudentViewModel
            {
                ClassGroups = await GetClassGroupsSelectListAsync()
            };
            return View(model);
        }

        private async Task<IEnumerable<SelectListItem>> GetClassGroupsSelectListAsync()
        {
            var groups = await _classGroupRepository.GetAllAsync();
            return groups.Select(g => new SelectListItem
            {
                Value = g.ClassGroupId.ToString(),
                Text = g.Description
            });
        }
    }
}