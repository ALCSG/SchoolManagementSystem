using Microsoft.AspNetCore.Identity;

namespace SchoolManagementSystem.Data.Entities
{
    public class AppUser : IdentityUser
    {

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfilePicturePath { get; set; }  
    }
}
