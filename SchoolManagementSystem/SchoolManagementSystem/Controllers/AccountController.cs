using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models;
using System.Security.Claims;

namespace SchoolManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMailHelper _mailHelper;

        public AccountController(SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment, IMailHelper mailHelper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _mailHelper = mailHelper;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Start", "Home");

            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Username);

                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                }

                return RedirectToAction("Start", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login information");

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                ProfilePicturePath = user.ProfilePicturePath
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            model.Email = user.Email;
            model.ProfilePicturePath = user.ProfilePicturePath;

            if (!ModelState.IsValid)
                return View(model);

            if (model.ProfilePictureFile != null)
            {
                if (model.ProfilePictureFile.Length == 0)
                {
                    ModelState.AddModelError(
                        nameof(model.ProfilePictureFile),
                        "Please select a non-empty image.");

                    return View(model);
                }

                try
                {
                    user.ProfilePicturePath =
                        await SaveProfilePhotoAsync(model.ProfilePictureFile);
                }
                catch (Exception ex) when (
                    ex is IOException or UnauthorizedAccessException)
                {
                    ModelState.AddModelError(
                        nameof(model.ProfilePictureFile),
                        "The profile picture could not be saved. Please try again.");

                    return View(model);
                }
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            var userUpdateResult = await _userManager.UpdateAsync(user);

            if (!userUpdateResult.Succeeded)
            {
                foreach (var error in userUpdateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            TempData["Message"] = "Changes saved successfully";

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            var model = new ForgotPasswordViewModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var link = Url.Action("ResetPassword", "Account", new { userId = user.Id, token }, protocol: Request.Scheme);

                if (link != null)
                {
                    var body = $@"
                            <p>Hello {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>
                            <p>Click the link below to choose a new password:</p>
                            <p><a href=""{link}"">Choose my password</a></p>
                            <p>If you did not ask for this, you can ignore this message.</p>";

                    await _mailHelper.SendEmailAsync(user.Email!, $"{user.FirstName} {user.LastName}", "Password reset", body);
                }
            }

            TempData["Message"] = "If the e-mail exists, we sent instructions to reset the password.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId.IsNullOrEmpty() || token.IsNullOrEmpty() )
                return NotFound();

            var model = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to reset the password. Please request a new reset link.");

                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user,model.Token,model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty,error.Description);
                }

                return View(model);
            }

            TempData["Message"] = "Password reset successfully. Please sign in with your new password.";

            return RedirectToAction(nameof(Login));
        }

        private async Task<string> SaveProfilePhotoAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "profiles");

            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/profiles/{fileName}";
        }
    }
}