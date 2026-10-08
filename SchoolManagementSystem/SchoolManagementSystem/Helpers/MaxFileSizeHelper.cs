using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Helpers
{
    public class MaxFileSizeHelper : ValidationAttribute
    {
        private readonly long _maxBytes;

        public MaxFileSizeHelper(int maxMegabytes)
        {
            _maxBytes = maxMegabytes * 1024L * 1024L;
            ErrorMessage = $"File size must not exceed {maxMegabytes} MB";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is IFormFile file && file.Length > _maxBytes)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
