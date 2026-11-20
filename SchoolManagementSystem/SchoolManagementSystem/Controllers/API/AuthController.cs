using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models.API;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SchoolManagementSystem.Controllers.API
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            DataContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(
                request.Email.Trim());

            if (user == null)
                return InvalidLogin();

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (!result.Succeeded)
                return InvalidLogin();

            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                return Unauthorized(new
                {
                    message = "Two-factor authentication is not supported by this app yet. Please use the website."
                });
            }

            if (!await _userManager.IsInRoleAsync(user,AppRoles.Student))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "This app is available to student accounts only."
                    });
            }

            var hasStudent = await _context.Set<Student>().AnyAsync(student => student.AppUserId == user.Id);

            if (!hasStudent)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "No student record is associated with this account."
                    });
            }

            return Ok(CreateToken(user));
        }

        private LoginResponse CreateToken(AppUser user)
        {
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = _configuration["Jwt:Key"];

            var expirationMinutes =
                _configuration.GetValue<int?>("Jwt:ExpirationMinutes");

            if (string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience) ||
                string.IsNullOrWhiteSpace(key) ||
                Encoding.UTF8.GetByteCount(key) < 64 ||
                expirationMinutes is null ||
                expirationMinutes <= 0 ||
                expirationMinutes > 60)
            {
                throw new InvalidOperationException("JWT configuration is missing or invalid. ExpirationMinutes must be between 1 and 60.");
            }

            var now = DateTime.UtcNow;
            var expiresAtUtc = now.AddMinutes(expirationMinutes.Value);
            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim("name", fullName),
                new Claim("role", AppRoles.Student)
            };

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new LoginResponse
            {
                AccessToken =
                    new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAtUtc = expiresAtUtc,
                FullName = fullName
            };
        }

        private UnauthorizedObjectResult InvalidLogin()
        {
            return Unauthorized(new
            {
                message = "Invalid email or password, or login unavailable."
            });
        }
    }
}