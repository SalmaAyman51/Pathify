using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pathify.Models;

namespace Pathify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentDashboardController : ControllerBase
    {
        private readonly PathifyContext _context;

        public StudentDashboardController(PathifyContext context)
        {
            _context = context;
        }
        [HttpGet("enrolled-courses/{ssn}")]
        public async Task<IActionResult> GetPassedCoursesCount(string ssn)
        {
            var count = await _context.Enrollments
                .Where(e => e.StudentSsn != null &&
                            e.StudentSsn.Trim() == ssn.Trim() &&
                            e.Passed == PassStatus.Passed)
                .CountAsync();

            return Ok(count);
        }

        [HttpGet("completed-credits/{ssn}")]
        public async Task<IActionResult> GetCompletedCredits(string ssn)
        {
            var totalCredits = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentSsn.Trim() == ssn.Trim()
                            && e.Passed == PassStatus.Passed)
                .SumAsync(e => e.Course.CreditHours);

            return Ok(new
            {
                completedCredits = totalCredits
            });
        }

        [HttpGet("search-student-courses/{query}")]
        public async Task<ActionResult> SearchStudentCourses(string query)
        {
            // ✅ جيب الـ SSN من الـ Token
            var ssn = User.FindFirst("SSN")?.Value;
            if (ssn == null) return Unauthorized("Invalid token");

            var student = await _context.Students.FindAsync(ssn);
            if (student == null) return NotFound("Student not found");

            var courses = await _context.Enrollments
                .Where(e => e.StudentSsn == ssn &&
                           (e.CourseId.StartsWith(query) ||
                            e.Course.CourseName.StartsWith(query)))
                .Select(e => new
                {
                    e.CourseId,
                    e.Course.CourseName,
                    e.Course.CourseSemester,
                    e.Course.CreditHours,
                    e.Passed
                })
                .ToListAsync();

            if (!courses.Any())
                return NotFound("No courses found");

            return Ok(courses);
        }
       
    }
    }
