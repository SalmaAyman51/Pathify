using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pathify.Models;
using Microsoft.AspNetCore.Identity;

namespace Pathify.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly PathifyContext _context;

        public AdminController(PathifyContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        private readonly UserManager<ApplicationUser> _userManager;

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("You have accessed the Admin controller.");
        }
        [HttpPost("approve/{userId}")]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            if (user.IsApproved)
                return BadRequest("Already approved");

            // ✅ نوافق عليه
            user.IsApproved = true;
            await _userManager.UpdateAsync(user);

            // ✅ نحوله لـ Student
            var student = new Student
            {
                StudentSsn = user.SSN,
                Fname = user.FirstName,
                Lname = user.LastName,
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email,
                Gpa = (decimal?)user.GPA,
                BirthDate = DateOnly.FromDateTime(user.BirthDate),
                EnrollmentYear = user.EnrollmentYear,
                AcademicLevel = user.AcademicLevel,
                IsApproved = true
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Ok("User approved and added as student");
        }

        [HttpGet("pending")]
        public IActionResult GetPendingUsers()
        {
            var users = _userManager.Users
                .Where(u => !u.IsApproved)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.SSN
                }).ToList();

            return Ok(users);
        }
    }
}
