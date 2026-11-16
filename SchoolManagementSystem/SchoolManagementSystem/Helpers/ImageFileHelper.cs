using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SchoolManagementSystem.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ImageFileHelper : ValidationAttribute
    {
        private readonly int _maxFileSizeMb;
        private readonly long _maxFileSizeBytes;

        public ImageFileHelper(int maxFileSizeMb = 2)
        {
            if (maxFileSizeMb <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFileSizeMb),"The maximum file size must be greater than zero.");
            }

            _maxFileSizeMb = maxFileSizeMb;
            _maxFileSizeBytes = maxFileSizeMb * 1024L * 1024L;
        }

        protected override ValidationResult? IsValid(object? value,ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is not IFormFile file)
            {
                return new ValidationResult("The selected file is invalid.");
            }

            if (file.Length == 0)
            {
                return new ValidationResult("Please select a non-empty image.");
            }

            if (file.Length > _maxFileSizeBytes)
            {
                return new ValidationResult($"The image must not exceed {_maxFileSizeMb} MB.");
            }

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                return new ValidationResult("Only JPG, JPEG and PNG images are allowed.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new BinaryReader(stream);

                var header = reader.ReadBytes(8);

                var isJpeg =
                    header.Length >= 3 &&
                    header[0] == 0xFF &&
                    header[1] == 0xD8 &&
                    header[2] == 0xFF;

                var pngSignature = new byte[]
                {
                    0x89, 0x50, 0x4E, 0x47,
                    0x0D, 0x0A, 0x1A, 0x0A
                };

                var isPng =
                    header.Length == pngSignature.Length &&
                    header.SequenceEqual(pngSignature);

                var matchesExtension = extension switch
                {
                    ".jpg" or ".jpeg" => isJpeg,
                    ".png" => isPng,
                    _ => false
                };

                if (!matchesExtension)
                {
                    return new ValidationResult("The file content does not match the selected image format.");
                }

                return ValidationResult.Success;
            }
            catch (IOException)
            {
                return new ValidationResult("The image could not be read. Please select the file again.");
            }
        }
    }
}