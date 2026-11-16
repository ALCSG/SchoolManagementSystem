using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models;
using System.Security.Claims;

namespace SchoolManagementSystem.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IClassGroupRepository _classGroupRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IAlertRepository _alertRepository;
        private readonly ISchoolSettingsRepository _settingsRepository;
        private readonly DataContext _context;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            IStudentRepository studentRepository,
            IClassGroupRepository classGroupRepository,
            UserManager<AppUser> userManager,
            IWebHostEnvironment environment,
            IAlertRepository alertRepository,
            ISchoolSettingsRepository settingsRepository,
            DataContext context,
            ILogger<StudentController> logger)
        {
            _studentRepository = studentRepository;
            _classGroupRepository = classGroupRepository;
            _userManager = userManager;
            _environment = environment;
            _alertRepository = alertRepository;
            _settingsRepository = settingsRepository;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Index()
        {
            var students = await _studentRepository.GetAllWithDetailsAsync();
            return View(students);
        }

        [HttpGet]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Create()
        {
            var model = new StudentViewModel
            {
                ClassGroups = await GetClassGroupsSelectListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentViewModel model)
        {
            if (model.PhotoFile == null || model.PhotoFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.PhotoFile),
                    "Student photo is required.");
            }

            if (!ModelState.IsValid)
            {
                model.ClassGroups = await GetClassGroupsSelectListAsync();
                return View(model);
            }

            var appUserId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(appUserId))
                return Forbid();

            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

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
                    Message =
                        $"Create a user account for student " +
                        $"{student.FirstName} {student.LastName} " +
                        $"with email: {student.Email}.",
                    ReportedByUserId = appUserId,
                    StudentId = student.StudentId,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _alertRepository.AddAsync(alert);
                await _alertRepository.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex is DbUpdateException or IOException or UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Failed to create the student and the account request alert.");

                ModelState.AddModelError(string.Empty, "The student could not be created. Please try again.");

                model.ClassGroups = await GetClassGroupsSelectListAsync();

                return View(model);
            }

            TempData["Message"] = $"Student created successfully. An account request was sent to the administrator.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentRepository.GetByIdWithDetailsAsync(id);

            if (student == null)
                return NotFound();

            if (student.IsArchived)
            {
                TempData["Message"] = "This student is archived. The record is available for consultation only.";

                return RedirectToAction(nameof(Details), new { id = student.StudentId });
            }

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
        [Authorize(Roles = AppRoles.Employee)]
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

            if (student.IsArchived)
            {
                TempData["Message"] = "This student is archived. The record is available for consultation only.";

                return RedirectToAction(nameof(Details),new { id = student.StudentId });
            }

            if (model.ClassGroupId != student.ClassGroupId)
            {
                var targetClassGroup = await _classGroupRepository
                    .GetByIdWithDetailsAsync(model.ClassGroupId);

                if (targetClassGroup == null)
                {
                    ModelState.AddModelError(
                        nameof(model.ClassGroupId), "The selected class group does not exist.");
                }
                else
                {
                    var availableSubjectIds = targetClassGroup.Course.Subjects
                        .Select(subject => subject.SubjectId)
                        .ToHashSet();

                    var incompatibleSubjects = student.Enrollments
                        .Where(enrollment =>
                            !availableSubjectIds.Contains(enrollment.SubjectId))
                        .Select(enrollment => enrollment.Subject.Name)
                        .Distinct()
                        .OrderBy(name => name)
                        .ToList();

                    if (incompatibleSubjects.Count > 0)
                    {
                        ModelState.AddModelError(
                            nameof(model.ClassGroupId),
                            "The student cannot be moved to this class group because " +
                            "the destination course does not include these enrolled " +
                            $"subjects: {string.Join(", ", incompatibleSubjects)}. " +
                            "Please contact the administrator to resolve the conflict.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    model.CurrentPhotoPath = student.StudentPhotoPath;
                    model.ClassGroups = await GetClassGroupsSelectListAsync();

                    return View(model);
                }
            }

            student.FirstName = model.FirstName;
            student.LastName = model.LastName;
            student.Email = model.Email;
            student.ClassGroupId = model.ClassGroupId;

            try
            {
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    student.StudentPhotoPath = await SavePhotoAsync(model.PhotoFile);
                }

                _studentRepository.Update(student);
                await _studentRepository.SaveChangesAsync();

                TempData["Message"] = "Student updated successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "The student could not be updated. Please try again.");

                model.ClassGroups = await GetClassGroupsSelectListAsync();

                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.Employee + "," + AppRoles.Student)]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentRepository.GetByIdWithDetailsAsync(id);

            if (student == null)
                return NotFound();

            if (!User.IsInRole(AppRoles.Employee))
            {
                var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(appUserId) ||
                    student.AppUserId != appUserId)
                {
                    return Forbid();
                }
            }

            var settings = await _settingsRepository.GetSettingsAsync();

            var model = new StudentDetailsViewModel
            {
                Student = student,
                MaxAbsencePercentage = settings.MaxAbsencePercentage
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentRepository.GetByIdWithDetailsAsync(id);
            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = AppRoles.Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _studentRepository
                .GetByIdWithDetailsAsync(id);

            if (student == null)
                return NotFound();

            if (student.IsArchived)
            {
                TempData["Message"] = "This student is already archived.";
                return RedirectToAction(nameof(Details), new { id = student.StudentId });
            }

            try
            {
                student.IsArchived = true;
                student.ArchivedAt = DateTime.UtcNow;

                _studentRepository.Update(student);
                await _studentRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,"Failed to archive student {StudentId}.",id);

                ModelState.AddModelError(string.Empty,"The student could not be archived. Please try again.");

                return View("Delete", student);
            }

            TempData["Message"] = "Student archived successfully.";

            return RedirectToAction(nameof(Details), new { id = student.StudentId });
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyLearning()
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(appUserId))
                return Forbid();

            var student = await _studentRepository.GetByAppUserIdWithDetailsAsync(appUserId);

            if (student == null)
                return NotFound();

            var settings = await _settingsRepository.GetSettingsAsync();

            var model = new MyLearningViewModel
            {
                Student = student,
                MaxAbsencePercentage = settings!.MaxAbsencePercentage
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var student = await _studentRepository
                .GetByIdWithDetailsAsync(id);

            if (student == null)
                return NotFound();

            if (!student.IsArchived)
            {
                TempData["Message"] = "This student is already active.";

                return RedirectToAction(nameof(Details),new { id = student.StudentId });
            }

            try
            {
                student.IsArchived = false;
                student.ArchivedAt = null;

                _studentRepository.Update(student);
                await _studentRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,"Failed to reactivate student {StudentId}.",id);

                TempData["Error"] = "The student could not be reactivated. Please try again.";

                return RedirectToAction(nameof(Details),new { id = student.StudentId });
            }

            TempData["Message"] = "Student reactivated successfully.";

            return RedirectToAction(nameof(Details),new { id = student.StudentId });
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