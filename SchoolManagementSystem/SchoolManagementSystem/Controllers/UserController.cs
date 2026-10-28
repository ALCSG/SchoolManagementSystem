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

namespace SchoolManagementSystem.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRoleHelper _roleHelper;
        private readonly IStudentRepository _studentRepository;
        private readonly DataContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserManager<AppUser> userManager,
            IRoleHelper roleHelper,
            IStudentRepository studentRepository,
            DataContext context,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _roleHelper = roleHelper;
            _studentRepository = studentRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            var usersList = new List<UserListViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var model = new UserListViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Roles = roles
                };

                usersList.Add(model);
            }
                
            return View(usersList);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? studentId)
        {
            var model = new CreateUserViewModel();

            if (studentId.HasValue)
            {
                var student = await _studentRepository
                    .GetByIdWithDetailsAsync(studentId.Value);

                if (student == null)
                    return NotFound();

                if (student.AppUserId != null)
                {
                    TempData["Message"] = "This student already has a user account.";
                    return RedirectToAction(nameof(Create));
                }

                model.StudentId = student.StudentId;
                model.Role = AppRoles.Student;
                model.FirstName = student.FirstName;
                model.LastName = student.LastName;
                model.Email = student.Email;
            }

            await LoadCreateOptionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            await LoadCreateOptionsAsync(model);

            try
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                Student? student = null;

                if (model.StudentId.HasValue)
                {
                    student = await _studentRepository
                        .GetByIdWithDetailsAsync(model.StudentId.Value);

                    if (student == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "The selected student was not found.");

                        return View(model);
                    }

                    if (student.AppUserId != null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "This student already has a user account.");

                        return View(model);
                    }

                    model.Role = AppRoles.Student;
                    model.FirstName = student.FirstName;
                    model.LastName = student.LastName;
                    model.Email = student.Email;

                    ModelState.Remove(nameof(model.Role));
                    ModelState.Remove(nameof(model.FirstName));
                    ModelState.Remove(nameof(model.LastName));
                    ModelState.Remove(nameof(model.Email));

                    TryValidateModel(model);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(model.Role) ||
                        !AppRoles.All.Contains(model.Role))
                    {
                        ModelState.AddModelError(nameof(model.Role),
                            "Please select a valid role.");
                    }
                    else if (model.Role == AppRoles.Student)
                    {
                        ModelState.AddModelError(nameof(model.Role),
                            "Please select a student from the pending students panel.");
                    }
                }

                if (!ModelState.IsValid)
                    return View(model);

                var appUser = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var createResult = await _userManager.CreateAsync(appUser);

                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(model);
                }

                var roleResult = await _roleHelper
                    .AddUserToRoleAsync(appUser, model.Role);

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(model);
                }

                if (student != null)
                {
                    student.AppUserId = appUser.Id;

                    _studentRepository.Update(student);
                    await _studentRepository.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                TempData["Message"] = "User account created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create a user account for student {StudentId}.",
                    model.StudentId);

                ModelState.AddModelError(string.Empty,
                    "The user account could not be created. Please try again.");

                return View(model);
            }
        }

        private IEnumerable<SelectListItem> GetRolesSelectList()
        {
            return AppRoles.All
                .Where(role => role != AppRoles.Student)
                .Select(role => new SelectListItem
                {
                    Value = role,
                    Text = role
                });
        }

        private async Task LoadCreateOptionsAsync(CreateUserViewModel model)
        {
            model.Roles = GetRolesSelectList();

            var students = await _studentRepository
                .GetStudentsWithoutAccountAsync();

            model.PendingStudents = students.Select(student =>
                new PendingStudentViewModel
                {
                    StudentId = student.StudentId,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Email = student.Email
                })
                .ToList();
        }
    }
}
