using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Models;
using System.Diagnostics;

namespace SchoolManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("Error/{statusCode?}")]
        public IActionResult Error(int? statusCode)
        {
            var code = statusCode ?? 500;

            var model = code switch
            {
                403 => new ErrorViewModel { StatusCode = 403, Title = "Access denied", Message = "You don't have permission to access this page." },
                404 => new ErrorViewModel { StatusCode = 404, Title = "Page not found", Message = "The page you're looking for doesn't exist or was moved." },
                _ => new ErrorViewModel { StatusCode = code, Title = "An error occured", Message = "Something went wrong. Try again later." }
            };

            Response.StatusCode = code;
            return View(model);
        }
    }
}
