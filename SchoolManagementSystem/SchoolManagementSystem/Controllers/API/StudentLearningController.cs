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
    [Route("api/students/me/learning")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = AppRoles.Student)]
    public class StudentLearningController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IStudentRepository _studentRepository;
        private readonly DataContext _context;

        public StudentLearningController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IStudentRepository studentRepository,
            DataContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _studentRepository = studentRepository;
            _context = context;
        }

        [HttpGet]
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

        private UnauthorizedObjectResult InvalidSession()
        {
            return Unauthorized(new
            {
                message = "Your session is no longer valid. Please sign in again."
            });
        }
    }
}