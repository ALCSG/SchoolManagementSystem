using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data.Entities;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    [Authorize(Roles = AppRoles.Employee)]
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
            if (!await PopulateViewModelAsync(model))
                return NotFound();

            var selectedSubjectIds = (model.SelectedSubjectIds ?? new List<int>())
                .Distinct()
                .ToList();

            if (!selectedSubjectIds.Any())
            {
                ModelState.AddModelError(nameof(model.SelectedSubjectIds), "Please select at least one subject");
            }

            if (!ModelState.IsValid)
                return View(model);

            var availableSubjects = await _enrollmentRepository.GetAvailableSubjectsForStudentAsync(model.StudentId);

            var availableSubjectIds = availableSubjects
                .Select(s => s.SubjectId)
                .ToHashSet();

            foreach (var subjectId in selectedSubjectIds)
            {
                if (await _enrollmentRepository
                    .EnrollmentExistsAsync(model.StudentId, subjectId))
                {
                    ModelState.AddModelError(
                        nameof(model.SelectedSubjectIds),
                        "The student is already enrolled in one or more selected subjects.");

                    break;
                }

                if (!availableSubjectIds.Contains(subjectId))
                {
                    ModelState.AddModelError(
                        nameof(model.SelectedSubjectIds),
                        "One or more selected subjects are not available for this student.");

                    break;
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            foreach (var subjectId in selectedSubjectIds)
            {
                var enrollment = new Enrollment
                {
                    StudentId = model.StudentId,
                    SubjectId = subjectId
                };

                await _enrollmentRepository.AddAsync(enrollment);
            }

            await _enrollmentRepository.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Student",
                new { id = model.StudentId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var enrollment = await _enrollmentRepository.GetByIdWithDetailsAsync(id);

            if (enrollment == null)
                return NotFound();

            var model = new EnrollmentViewModel()
            {
                EnrollmentId = enrollment.EnrollmentId,
                StudentId = enrollment.StudentId,
                StudentName = $"{enrollment.Student.AppUser.FirstName} {enrollment.Student.AppUser.LastName}",
                SubjectName = enrollment.Subject.Name,
                FinalGrade = enrollment.FinalGrade,
                AbsenceCount = enrollment.AbsenceCount
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EnrollmentViewModel model)
        {
            var enrollment = await _enrollmentRepository.GetByIdWithDetailsAsync(model.EnrollmentId);

            if (enrollment == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.StudentName = $"{enrollment.Student.AppUser.FirstName} {enrollment.Student.AppUser.LastName}";
                model.SubjectName = enrollment.Subject.Name;

                return View(model);
            }

            enrollment.AbsenceCount = model.AbsenceCount;
            enrollment.FinalGrade = model.FinalGrade;

            await _enrollmentRepository.SaveChangesAsync();

            return RedirectToAction("Details", "Student", new { id = enrollment.StudentId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var enrollment = await _enrollmentRepository.GetByIdWithDetailsAsync(id);

            if (enrollment == null)
                return NotFound();

            return View(enrollment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _enrollmentRepository.GetByIdWithDetailsAsync(id);

            if (enrollment == null)
                return NotFound();

            if (enrollment.FinalGrade != null)
            {
                ModelState.AddModelError(nameof(enrollment.SubjectId), "The selected enrollment can't be deleted due to having a final grade");
                return View(enrollment);
            }

            try
            {
                _enrollmentRepository.Delete(enrollment);
                await _enrollmentRepository.SaveChangesAsync();
                return RedirectToAction("Details", "Student", new { id = enrollment.StudentId });
            }
            catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty, "This enrollment could not be deleted because it is still in use.");
                return View("Delete", enrollment);
            }
        }

        private async Task<bool> PopulateViewModelAsync(EnrollmentViewModel model)
        {
            var student = await _studentRepository.GetByIdWithDetailsAsync(model.StudentId);

            if (student == null)
                return false;

            model.StudentName = $"{student.FirstName} {student.LastName}";
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