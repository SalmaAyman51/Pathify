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
                            e.Passed == true)
                .CountAsync();

            return Ok(count);
        }
        [HttpGet("completed-credits/{ssn}")]
        public async Task<IActionResult> GetCompletedCredits(string ssn)
        {
            var totalCredits = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentSsn.Trim() == ssn.Trim()
                            && e.Passed == true) 
                .SumAsync(e => e.Course.CreditHours);

            return Ok(new
            {
                completedCredits = totalCredits
            });
        }

    }
    }
