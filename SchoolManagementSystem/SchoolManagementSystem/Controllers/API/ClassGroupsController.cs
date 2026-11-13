using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Data.Interface;
using SchoolManagementSystem.Helpers;

namespace SchoolManagementSystem.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Employee)]
    public class ClassGroupsController : ControllerBase
    {
        private readonly IClassGroupRepository _classGroupRepository;

        public ClassGroupsController(IClassGroupRepository classGroupRepository)
        {
            _classGroupRepository = classGroupRepository;
        }

        [HttpGet("{id:int}/students")]
        public async Task<IActionResult> GetStudentsByClassGroup(int id)
        {
            var classGroup = await _classGroupRepository.GetByIdWithDetailsAsync(id);

            if (classGroup == null)
                return NotFound();

            var students = classGroup.Students
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .Select(s => new
                {
                    s.StudentId,
                    s.FirstName,
                    s.LastName
                })
                .ToList();

            return Ok(students);
        }
    }
}