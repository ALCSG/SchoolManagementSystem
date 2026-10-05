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
            if (model.PhotoFile == null || model.PhotoFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.PhotoFile), "Student photo is required");
            }

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

        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentRepository.GetByIdWithDetailsAsync(id);

            if (student == null)
                return NotFound();

            var model = new StudentViewModel
            {
                StudentId = student.StudentId,
                FirstName = student.AppUser.FirstName,
                LastName = student.AppUser.LastName,
                Email = student.AppUser.Email!,
                ClassGroupId = student.ClassGroupId,
                CurrentPhotoPath = student.StudentPhotoPath,
                ClassGroups = await GetClassGroupsSelectListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ClassGroups = await GetClassGroupsSelectListAsync();
                return View(model);
            }

            var student = await _studentRepository.GetByIdWithDetailsAsync(model.StudentId);

            if (student == null)
                return NotFound();

            student.AppUser.FirstName = model.FirstName;
            student.AppUser.LastName = model.LastName;
            student.AppUser.Email = model.Email;
            student.AppUser.UserName = model.Email;

            var userUpdateResult = await _userManager.UpdateAsync(student.AppUser);

            if (!userUpdateResult.Succeeded)
            {
                foreach (var error in userUpdateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.ClassGroups = await GetClassGroupsSelectListAsync();
                return View(model);
            }

            student.ClassGroupId = model.ClassGroupId;

            if(model.PhotoFile != null && model.PhotoFile.Length > 0)
                student.StudentPhotoPath = await SavePhotoAsync(model.PhotoFile);

            _studentRepository.Update(student);
            await _studentRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}