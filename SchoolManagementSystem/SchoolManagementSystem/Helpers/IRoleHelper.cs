using Microsoft.AspNetCore.Identity;
using SchoolManagementSystem.Data.Entities;

namespace SchoolManagementSystem.Helpers
{
    public interface IRoleHelper
    {
        Task<IdentityResult> CheckRoleAsync(string roleName);

        Task<IdentityResult> AddUserToRoleAsync(AppUser user, string roleName);

        Task<bool> IsUserInRoleAsync(AppUser user, string roleName);

        Task SeedRolesAsync(IEnumerable<string> roleNames);

        Task<IdentityResult> SeedAdminAsync(string email, string password, string firstName, string lastName);
    }
}
