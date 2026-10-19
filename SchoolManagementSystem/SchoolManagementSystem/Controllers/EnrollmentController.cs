using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public EnrollmentController(IStudentRepository studentRepository, IEnrollmentRepository enrollmentRepository)
        {
            _studentRepository = studentRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int studentId)
        {
            var model = new EnrollmentViewModel { StudentId = studentId };

            if (!await PopulateViewModelAsync(model))
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnrollmentViewModel model)
        {
            if (await _enrollmentRepository.EnrollmentExistsAsync(model.StudentId, model.SubjectId))
            {
                ModelState.AddModelError(nameof(model.SubjectId), "The selected student is already enrolled in this subject");
            }
            else
            {
                var availableSubjects = await _enrollmentRepository
                    .GetAvailableSubjectsForStudentAsync(model.StudentId);

                if (!availableSubjects.Any(s => s.SubjectId == model.SubjectId))
                {
                    ModelState.AddModelError(nameof(model.SubjectId), "The selected subject is not available for this student");
                }
            }

            if (!ModelState.IsValid)
            {
                if (!await PopulateViewModelAsync(model))
                    return NotFound();

                return View(model);
            }

            var enrollment = new Enrollment
            {
                StudentId = model.StudentId,
                SubjectId = model.SubjectId
            };

            await _enrollmentRepository.AddAsync(enrollment);
            await _enrollmentRepository.SaveChangesAsync();

            return RedirectToAction("Details", "Student", new { id = model.StudentId });
        }

        private async Task<bool> PopulateViewModelAsync(EnrollmentViewModel model)
        {
            var student = await _studentRepository.GetByIdWithDetailsAsync(model.StudentId);

            if (student == null)
                return false;

            model.StudentName = $"{student.AppUser.FirstName} {student.AppUser.LastName}";
            model.SubjectsList = await GetAvailableSubjectsSelectListAsync(model.StudentId);

            return true;
        }

        private async Task<IEnumerable<SelectListItem>> GetAvailableSubjectsSelectListAsync(int studentId)
        {
            var availableSubjects = await _enrollmentRepository.GetAvailableSubjectsForStudentAsync(studentId);

            return availableSubjects.Select(s => new SelectListItem
            {
                Value = s.SubjectId.ToString(),
                Text = s.Name
            });
        }
    }
}