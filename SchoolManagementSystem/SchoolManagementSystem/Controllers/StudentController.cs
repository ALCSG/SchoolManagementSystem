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
        private readonly IWebHostEnvironment _environment;

        public StudentController(
            IStudentRepository studentRepository,
            IClassGroupRepository classGroupRepository,
            UserManager<AppUser> userManager,
            IWebHostEnvironment environment)
        {
            _studentRepository = studentRepository;
            _classGroupRepository = classGroupRepository;
            _userManager = userManager;
            _environment = environment;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ClassGroups = await GetClassGroupsSelectListAsync();
                return View(model);
            }

            var appUser = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            // TODO: replace with a secure generated password + email flow
            var tempPassword = "Temp#12345";
            var result = await _userManager.CreateAsync(appUser, tempPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.ClassGroups = await GetClassGroupsSelectListAsync();
                return View(model);
            }

            var photoPath = await SavePhotoAsync(model.PhotoFile!);

            var student = new Student
            {
                AppUserId = appUser.Id,
                ClassGroupId = model.ClassGroupId,
                StudentPhotoPath = photoPath
            };

            await _studentRepository.AddAsync(student);
            await _studentRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SavePhotoAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "students");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/students/{fileName}";
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