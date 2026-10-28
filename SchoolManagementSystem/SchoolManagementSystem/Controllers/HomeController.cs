using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICourseRepository _courseRepository;

        public HomeController(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction(nameof(Start));

            var courses = await _courseRepository.GetAllWithDetailsAsync();

            return View(courses);
        }

        [Authorize]
        public IActionResult Start()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [Route("Error/{statusCode?}")]
        public IActionResult Error(int? statusCode)
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var code = statusCode ?? 500;

            var model = code switch
            {
                403 => new ErrorViewModel { StatusCode = 403, Title = "Access denied", Message = "You do not have permission to access this page.", RequestId = requestId },
                404 => new ErrorViewModel { StatusCode = 404, Title = "Page not found", Message = "The page you are looking for does not exist or was moved.", RequestId = requestId },
                _ => new ErrorViewModel { StatusCode = code, Title = "Something went wrong", Message = "An unexpected error occurred. Please try again later.", RequestId = requestId }
            };

            Response.StatusCode = code;

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            var courses = await _courseRepository.GetAllWithDetailsAsync();
            return View(courses);
        }
    }
}