using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRoleHelper _roleHelper;

        public UserController(UserManager<AppUser> userManager, IRoleHelper roleHelper)
        {
            _userManager = userManager;
            _roleHelper = roleHelper;
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
        public IActionResult Create()
        {
            var model = new CreateUserViewModel
            {
                Roles = GetRolesSelectList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Role) || !AppRoles.All.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Please select a valid role.");
            }

            if (!ModelState.IsValid)
            {
                model.Roles = GetRolesSelectList();
                return View(model);
            }

            var appUser = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(appUser);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.Roles = GetRolesSelectList();
                return View(model);
            }

            var roleResult = await _roleHelper.AddUserToRoleAsync(appUser, model.Role);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(appUser);

                foreach (var error in roleResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.Roles = GetRolesSelectList();
                return View(model);
            }

            TempData["Message"] = "User account created successfully.";

            return RedirectToAction(nameof(Index));
        }

        private IEnumerable<SelectListItem> GetRolesSelectList()
        {
            return AppRoles.All.Select(role => new SelectListItem
            {
                Value = role,
                Text = role
            });
        }
    }
}
