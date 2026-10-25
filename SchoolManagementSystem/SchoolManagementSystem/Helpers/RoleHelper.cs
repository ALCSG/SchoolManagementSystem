using Microsoft.AspNetCore.Identity;
using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Helpers
{
    public class RoleHelper : IRoleHelper
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleHelper(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IdentityResult> CheckRoleAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                return IdentityResult.Success;

            return await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        public async Task<IdentityResult> AddUserToRoleAsync(AppUser user, string roleName)
            => await _userManager.AddToRoleAsync(user, roleName);

        public async Task<bool> IsUserInRoleAsync(AppUser user, string roleName)
            => await _userManager.IsInRoleAsync(user, roleName);

        public async Task SeedRolesAsync(IEnumerable<string> roleNames)
        {
            foreach (var roleName in roleNames)
            {
                var result = await CheckRoleAsync(roleName);

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Could not create role '{roleName}': {errors}");
                }
            }
        }

        public async Task<IdentityResult> SeedAdminAsync(string email, string password, string firstName, string lastName)
        {
            var admin = await _userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(admin, password);

                if (!createResult.Succeeded)
                    return createResult;
            }

            if (await _userManager.IsInRoleAsync(admin, AppRoles.Admin))
                return IdentityResult.Success;

            return await _userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }
}