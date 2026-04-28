using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pathify.Models;
using Pathify.DTOs;

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
        private object _roleManager;

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
                //Fname = user.FirstName,
                //Lname = user.LastName,
                //FullName = user.FirstName + " " + user.LastName,
                Email = user.Email,
                //Gpa = (decimal?)user.GPA,
                //BirthDate = DateOnly.FromDateTime(user.BirthDate),
                //EnrollmentYear = user.EnrollmentYear,
                //AcademicLevel = user.AcademicLevel,
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
                    //u.FirstName,
                    //u.LastName,
                    u.SSN
                }).ToList();

            return Ok(users);
        }

        [HttpGet("get-all-students")]
        public async Task<ActionResult> GetStudents([FromQuery] string? name)
        {
            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(s => s.FullName.Contains(name));
            }

            var result = await query.Select(s => new {
                s.StudentId,
                s.FullName,
                s.StudentSsn,
                s.AcademicLevel
            }).ToListAsync();

            return Ok(result);
        }


        //[HttpPost("add-student")]
        //public async Task<IActionResult> AddStudent([FromBody] Register model)
        //{
        //    // تأكد إن الـ Role موجود
        //    if (!await _roleManager.RoleExistsAsync("Student"))
        //        await _roleManager.CreateAsync(new IdentityRole("Student"));

        //    // إنشاء الـ User
        //    var user = new ApplicationUser
        //    {
        //        UserName = model.Email,
        //        Email = model.Email,
        //        PhoneNumber = model.PhoneNumber,
        //        FirstName = model.FirstName,
        //        LastName = model.LastName,
        //        SSN = model.SSN,
        //        StudentId = model.StudentId,
        //        EnrollmentYear = model.EnrollmentYear,
        //        GPA = model.GPA,
        //        AcademicLevel = model.AcademicLevel,
        //        BirthDate = model.BirthDate,
        //        Gender = model.Gender,
        //        IsApproved = true  // ✅ الأدمن بيضيفه مباشرة يبقى Approved تلقائياً
        //    };

        //    var result = await _userManager.CreateAsync(user, model.Password);

        //    if (!result.Succeeded)
        //        return BadRequest(result.Errors);

        //    await _userManager.AddToRoleAsync(user, "Student");

        //    // ✅ إضافة الطالب في جدول Students مباشرة
        //    var student = new Student
        //    {
        //        StudentSsn = user.SSN,
        //        StudentId = user.StudentId,
        //        Fname = user.FirstName,
        //        Lname = user.LastName,
        //        FullName = user.FirstName + " " + user.LastName,
        //        Email = user.Email,
        //        BirthDate = DateOnly.FromDateTime(user.BirthDate),
        //        Gender = user.Gender ?? "N/A",
        //        EnrollmentYear = user.EnrollmentYear,
        //        Gpa = (decimal?)user.GPA,
        //        AcademicLevel = user.AcademicLevel,
        //        IsApproved = true
        //    };

        //    _context.Students.Add(student);
        //    await _context.SaveChangesAsync();

        //    return Ok("Student added successfully");
        //}

        [HttpPut("update-student/{SSN}")]
        public async Task<ActionResult> UpdateStudent(string SSN, [FromBody] UpdateStudentDto updatedData)
        {
            var student = await _context.Students.FindAsync(SSN);
            if (student == null) return NotFound("Student not Found");

            if (updatedData.Email != null && updatedData.Email != student.Email)
            {
                var emailExists = await _context.Students
                    .FirstOrDefaultAsync(s => s.Email == updatedData.Email && s.StudentSsn != SSN);
                if (emailExists != null)
                    return BadRequest("Email already exists for another student");
            }

            if (updatedData.LevelId != null)
            {
                var levelExists = await _context.Levels.FindAsync(updatedData.LevelId);
                if (levelExists == null)
                    return BadRequest("LevelId does not exist");
            }

            if (updatedData.ProjectId != null)
            {
                var projectExists = await _context.Projects.FindAsync(updatedData.ProjectId);
                if (projectExists == null)
                    return BadRequest("ProjectId does not exist");
            }

            student.Fname = updatedData.Fname ?? student.Fname;
            student.Lname = updatedData.Lname ?? student.Lname;
            student.Email = updatedData.Email ?? student.Email;
            student.Gpa = updatedData.Gpa ?? student.Gpa;
            student.AcademicLevel = updatedData.AcademicLevel ?? student.AcademicLevel;
            student.EnrollmentYear = updatedData.EnrollmentYear ?? student.EnrollmentYear;
            student.BirthDate = updatedData.BirthDate ?? student.BirthDate;
            student.Gender = updatedData.Gender ?? student.Gender;
            student.TeamId = updatedData.TeamId ?? student.TeamId;
            student.ProjectId = updatedData.ProjectId ?? student.ProjectId;
            student.LevelId = updatedData.LevelId ?? student.LevelId;

            // ✅ StudentId مش بنغيره خالص عشان Unique وممكن يسبب مشاكل

            await _context.SaveChangesAsync();
            return Ok(new { message = "Edit is done" });
        }
        [HttpDelete("delete-student/{SSN}")]
        public async Task<ActionResult> DeleteStudent(string SSN)
        {
            var student = await _context.Students.FindAsync(SSN);
            if (student == null) return NotFound("Student not Found");

            // ✅ امسح أي Enrollments مرتبطة بالطالب الأول
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentSsn == SSN)
                .ToListAsync();

            if (enrollments.Any())
                _context.Enrollments.RemoveRange(enrollments);

            // ✅ بعدين امسح الطالب
            _context.Students.Remove(student);

            await _context.SaveChangesAsync();
            return Ok(new { message = "The Student has been deleted successfully" });
        }
    }
}
