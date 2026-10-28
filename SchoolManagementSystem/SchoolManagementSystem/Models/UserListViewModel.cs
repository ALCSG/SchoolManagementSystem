namespace SchoolManagementSystem.Models
{
    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}