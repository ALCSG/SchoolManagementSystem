using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models.API;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SchoolManagementSystem.Controllers.API
{
    [ApiController]
    [Route("api/students/me")]
    [Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = AppRoles.Student)]
    public class StudentController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IStudentRepository _studentRepository;
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IStudentRepository studentRepository,
            DataContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _studentRepository = studentRepository;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("learning")]
        [ResponseCache(NoStore = true,Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<MyLearningResponse>> Get()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(userId))
                return InvalidSession();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return InvalidSession();

            if (!await _signInManager.CanSignInAsync(user))
                return InvalidSession();

            if (await _userManager.IsLockedOutAsync(user))
                return InvalidSession();

            if (!await _userManager.IsInRoleAsync(user,AppRoles.Student))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "This account no longer has student access."
                    });
            }

            var student = await _studentRepository.GetByAppUserIdWithDetailsAsync(user.Id);

            if (student == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "No student record is associated with this account."
                    });
            }

            var maxAbsencePercentage = await _context
                .Set<SchoolSettings>()
                .AsNoTracking()
                .OrderBy(settings => settings.SchoolSettingsId)
                .Select(settings => (int?)settings.MaxAbsencePercentage)
                .FirstOrDefaultAsync();

            if (!maxAbsencePercentage.HasValue)
            {
                return Problem(statusCode:StatusCodes.Status503ServiceUnavailable,title: "School settings are unavailable.");
            }

            var response = new MyLearningResponse
            {
                FullName = $"{student.FirstName} {student.LastName}".Trim(),

                CourseName = student.ClassGroup?.Course?.Name,

                ClassGroupName = student.ClassGroup?.Description,

                IsArchived = student.IsArchived,

                MaxAbsencePercentage = maxAbsencePercentage.Value,

                Subjects = student.Enrollments
                    .OrderBy(enrollment => enrollment.Subject.Name)
                    .Select(enrollment => new MyLearningSubjectResponse
                    {
                        EnrollmentId = enrollment.EnrollmentId,
                        SubjectName = enrollment.Subject.Name,
                        TotalHours = enrollment.Subject.TotalHours,
                        AbsenceCount = enrollment.AbsenceCount,
                        FinalGrade = enrollment.FinalGrade
                    })
                    .ToList()
            };

            return Ok(response);
        }

        [HttpGet("profile")]
        public async Task<ActionResult<StudentProfileResponse>> GetProfile()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(userId))
                return InvalidSession();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return InvalidSession();

            var student = await _studentRepository
                .GetByAppUserIdWithDetailsAsync(user.Id);
            
            if (student == null)
                return NotFound(new
                {
                    message = "No student record is associated with this account."
                });

            var response = new StudentProfileResponse
            {
                FullName = $"{student.FirstName} {student.LastName}".Trim(),
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                PhotoUrl = user.ProfilePicturePath
            };

            return Ok(response);
        }

        [HttpPut("profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateStudentProfile request)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(userId))
                return InvalidSession();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return InvalidSession();

            if (!await _signInManager.CanSignInAsync(user))
                return InvalidSession();

            if (await _userManager.IsLockedOutAsync(user))
                return InvalidSession();

            if (!await _userManager.IsInRoleAsync(user, AppRoles.Student))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "This account no longer has student access."
                    });
            }

            var student = await _studentRepository
                .GetByAppUserIdWithDetailsAsync(user.Id);

            if (student == null)
            {
                return NotFound(new
                {
                    message = "No student record is associated with this account."
                });
            }

            string? newPhotoPath = null;

            if (request.ProfilePictureFile != null)
            {
                try
                {
                    newPhotoPath = await SaveProfilePhotoAsync(
                        request.ProfilePictureFile);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "The profile picture could not be saved. Please try again.");
                }
            }

            var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim();

            if (!string.Equals(
                user.PhoneNumber,
                phoneNumber,
                StringComparison.Ordinal))
            {
                user.PhoneNumberConfirmed = false;
            }

            user.PhoneNumber = phoneNumber;

            if (newPhotoPath != null)
            {
                user.ProfilePicturePath = newPhotoPath;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                if (newPhotoPath != null)
                {
                    TryDeleteNewProfilePhoto(newPhotoPath);
                }

                return BadRequest(new
                {
                    message = "Could not update your profile.",
                    errors = result.Errors
                        .Select(error => error.Description)
                        .ToArray()
                });
            }

            return NoContent();
        }

        private UnauthorizedObjectResult InvalidSession()
        {
            return Unauthorized(new
            {
                message = "Your session is no longer valid. Please sign in again."
            });
        }
        private async Task<string> SaveProfilePhotoAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "profiles");

            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            try
            {
                using var stream = new FileStream(filePath,FileMode.CreateNew);

                await file.CopyToAsync(stream);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                TryDeleteNewProfilePhoto($"/uploads/profiles/{fileName}");
                throw;
            }

            return $"/uploads/profiles/{fileName}";
        }

        private void TryDeleteNewProfilePhoto(string photoPath)
        {
            var filePath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "profiles",
                Path.GetFileName(photoPath));

            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex) when (
                ex is IOException or UnauthorizedAccessException)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}