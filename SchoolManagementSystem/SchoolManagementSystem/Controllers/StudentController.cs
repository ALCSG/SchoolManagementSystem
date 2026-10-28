using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IClassGroupRepository _classGroupRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IAlertRepository _alertRepository;

        public StudentController(
            IStudentRepository studentRepository,
            IClassGroupRepository classGroupRepository,
            UserManager<AppUser> userManager,
            IWebHostEnvironment environment,
            IAlertRepository alertRepository)
        {
            _studentRepository = studentRepository;
            _classGroupRepository = classGroupRepository;
            _userManager = userManager;
            _environment = environment;
            _alertRepository = alertRepository;
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

            try
            {
                var photoPath = await SavePhotoAsync(model.PhotoFile!);

                var student = new Student
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    ClassGroupId = model.ClassGroupId,
                    StudentPhotoPath = photoPath,
                    AppUserId = null
                };

                await _studentRepository.AddAsync(student);
                await _studentRepository.SaveChangesAsync();

                var alert = new Alert
                {
                    Title = "Student account required",
                    Message = $"Create a user account for student {student.FirstName} {student.LastName} with email: {student.Email}.",
                    ReportedByUserId = _userManager.GetUserId(User)!,
                    StudentId = student.StudentId,
                    IsResolved = false,
                    CreatedAt = DateTime.Now
                };

                await _alertRepository.AddAsync(alert);
                await _alertRepository.SaveChangesAsync();

                TempData["Message"] = "Student created successfully. An account request was sent to the administrator.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "The student could not be created. Please try again.");

                model.ClassGroups = await GetClassGroupsSelectListAsync();
                return View(model);
            }
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
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
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

            student.FirstName = model.FirstName;
            student.LastName = model.LastName;
            student.Email = model.Email;
            student.ClassGroupId = model.ClassGroupId;

            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                student.StudentPhotoPath = await SavePhotoAsync(model.PhotoFile);
            }

            _studentRepository.Update(student);
            await _studentRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentRepository.GetByIdWithDetailsAsync(id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentRepository.GetByIdWithDetailsAsync(id);
            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _studentRepository.GetByIdWithDetailsAsync(id);

            if (student == null)
                return NotFound();

            if (student.Enrollments.Any())
            {
                ModelState.AddModelError(string.Empty,
                    "This student cannot be deleted because they still have enrollments. Remove the enrollments first.");

                return View("Delete", student);
            }

            try
            {
                var appUser = student.AppUser;

                _studentRepository.Delete(student);
                await _studentRepository.SaveChangesAsync();

                if (appUser != null)
                {
                    var deleteResult = await _userManager.DeleteAsync(appUser);

                    if (!deleteResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty,
                            "The student was removed, but the login account could not be deleted.");

                        return View("Delete", student);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty,
                    "This student could not be deleted because it is still in use.");

                return View("Delete", student);
            }
        }
    }

}