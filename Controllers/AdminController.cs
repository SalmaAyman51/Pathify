using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pathify.DTOs;
using Pathify.Models;
using System.Linq;
using static Pathify.DTOs.CourseDTO;
using static Pathify.DTOs.CreateProfessorDTO;
using static Pathify.DTOs.UpdateProfessorsDTO;

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
        // ✅ إضافة Internal Professor
        // ✅ إضافة Internal Professor
        [HttpPost("add-internal-professor")]
        public async Task<IActionResult> AddInternalProfessor([FromBody] CreateInternalProfessorDTO model)
        {
            // التحقق من أن جميع الحقول المطلوبة موجودة
            if (string.IsNullOrWhiteSpace(model.SSN))
                return BadRequest("SSN is required");

            if (string.IsNullOrWhiteSpace(model.FullName))
                return BadRequest("Full Name is required");

            if (string.IsNullOrWhiteSpace(model.DeptName))
                return BadRequest("Department Name is required");

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                return BadRequest("Phone Number is required");

            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Password is required");

            // تأكد مش موجود قبل كده
            var exists = await _context.InternalProfessors
                .AnyAsync(p => p.InternalProfessorSsn == model.SSN);
            if (exists) return BadRequest("Professor already exists");

            // ضيفه في جدول InternalProfessors
            var professor = new InternalProfessor
            {
                InternalProfessorSsn = model.SSN,
                InternalProfessorName = model.FullName,
                DeptName = model.DeptName
            };
            _context.InternalProfessors.Add(professor);
            await _context.SaveChangesAsync();

            // ضيف الفون نمبر في جدول InternalProfessorPhone
            var phone = new InternalProfessorPhone
            {
                InternalProfessorSsn = model.SSN,
                PhoneNumber = model.PhoneNumber
            };
            _context.InternalProfessorPhones.Add(phone);
            await _context.SaveChangesAsync();

            // عمله account في Identity
            var user = new ApplicationUser
            {
                UserName = model.SSN,
                SSN = model.SSN,
                PhoneNumber = model.PhoneNumber,
                IsApproved = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Professor");

            return Ok("Internal Professor added successfully");
        }

        // ✅ إضافة External Professor
        [HttpPost("add-external-professor")]
        public async Task<IActionResult> AddExternalProfessor([FromBody] CreateExternalProfessorDTO model)
        {
            // التحقق من أن جميع الحقول المطلوبة موجودة
            if (string.IsNullOrWhiteSpace(model.SSN))
                return BadRequest("SSN is required");

            if (string.IsNullOrWhiteSpace(model.FullName))
                return BadRequest("Full Name is required");

            if (string.IsNullOrWhiteSpace(model.DeptName))
                return BadRequest("Department Name is required");

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                return BadRequest("Phone Number is required");

            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Password is required");

            // تأكد مش موجود قبل كده
            var exists = await _context.ExternalProfessors
                .AnyAsync(p => p.ExternalProfessorSsn == model.SSN);
            if (exists) return BadRequest("Professor already exists");

            // ضيفه في جدول ExternalProfessors
            var professor = new ExternalProfessor
            {
                ExternalProfessorSsn = model.SSN,
                ExternalProfessorName = model.FullName,
                DeptName = model.DeptName
            };
            _context.ExternalProfessors.Add(professor);
            await _context.SaveChangesAsync();

            // ضيف الفون نمبر في جدول ExternalProfessorPhone
            var phone = new ExternalProfessorPhone
            {
                ExternalProfessorSsn = model.SSN,
                PhoneNumber = model.PhoneNumber
            };
            _context.ExternalProfessorPhones.Add(phone);
            await _context.SaveChangesAsync();

            // عمله account في Identity
            var user = new ApplicationUser
            {
                UserName = model.SSN,
                SSN = model.SSN,
                PhoneNumber = model.PhoneNumber,
                IsApproved = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Professor");

            return Ok("External Professor added successfully");
        }

        [HttpGet("admin-get-all")]
        public async Task<ActionResult> GetAllForAdmin()
        {
            var internals = await _context.InternalProfessors
                .Include(i => i.InternalProfessorPhones)
                .AsNoTracking()
                .ToListAsync();

            var externals = await _context.ExternalProfessors
                .Include(e => e.ExternalProfessorPhones)
                .AsNoTracking()
                .ToListAsync();

            var combined = internals.Select(i => new {
                Id = i.InternalProfessorSsn,
                Name = i.InternalProfessorName,
                Dept = i.DeptName,
                Phone = i.InternalProfessorPhones.FirstOrDefault()?.PhoneNumber ?? "N/A"

            }).Concat(externals.Select(e => new {
                Id = e.ExternalProfessorSsn,
                Name = e.ExternalProfessorName,
                Dept = e.DeptName ?? "External",
                Phone = e.ExternalProfessorPhones.FirstOrDefault()?.PhoneNumber ?? "N/A"

            }));

            return Ok(combined);
        }

        [HttpPut("admin-update-internal/{ssn}")]
        public async Task<ActionResult> UpdateInternal(string ssn, [FromBody] UpdateInternalProfessorDto updatedProf)
        {
            var existingProf = await _context.InternalProfessors
                .Include(p => p.InternalProfessorPhones)
                .FirstOrDefaultAsync(p => p.InternalProfessorSsn == ssn);

            if (existingProf == null)
                return NotFound("Professor not found");

            if (!string.IsNullOrWhiteSpace(updatedProf.InternalProfessorName))
                existingProf.InternalProfessorName = updatedProf.InternalProfessorName;

            if (!string.IsNullOrWhiteSpace(updatedProf.DeptName))
                existingProf.DeptName = updatedProf.DeptName;

            if (updatedProf.InternalProfessorPhones != null && updatedProf.InternalProfessorPhones.Any())
            {
                var newPhoneNumber = updatedProf.InternalProfessorPhones.First().PhoneNumber;

                if (!string.IsNullOrWhiteSpace(newPhoneNumber))
                {
                    var existingPhone = existingProf.InternalProfessorPhones.FirstOrDefault();

                    if (existingPhone != null)
                        existingPhone.PhoneNumber = newPhoneNumber;
                    else
                        _context.InternalProfessorPhones.Add(new InternalProfessorPhone
                        {
                            InternalProfessorSsn = ssn,
                            PhoneNumber = newPhoneNumber
                        });

                    var user = await _userManager.FindByNameAsync(ssn);
                    if (user != null)
                    {
                        user.PhoneNumber = newPhoneNumber;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated Successfully" });
        }

        [HttpPut("admin-update-external/{ssn}")]
        public async Task<ActionResult> UpdateExternal(string ssn, [FromBody] UpdateExternalProfessorDto updatedProf)
        {
            var existingProf = await _context.ExternalProfessors
                .Include(p => p.ExternalProfessorPhones)
                .FirstOrDefaultAsync(p => p.ExternalProfessorSsn == ssn);

            if (existingProf == null)
                return NotFound("Professor not found");

            if (!string.IsNullOrWhiteSpace(updatedProf.ExternalProfessorName))
                existingProf.ExternalProfessorName = updatedProf.ExternalProfessorName;

            if (!string.IsNullOrWhiteSpace(updatedProf.DeptName))
                existingProf.DeptName = updatedProf.DeptName;

            if (updatedProf.ExternalProfessorPhones != null && updatedProf.ExternalProfessorPhones.Any())
            {
                var newPhoneNumber = updatedProf.ExternalProfessorPhones.First().PhoneNumber;

                if (!string.IsNullOrWhiteSpace(newPhoneNumber))
                {
                    var existingPhone = existingProf.ExternalProfessorPhones.FirstOrDefault();

                    if (existingPhone != null)
                        existingPhone.PhoneNumber = newPhoneNumber;
                    else
                        _context.ExternalProfessorPhones.Add(new ExternalProfessorPhone
                        {
                            ExternalProfessorSsn = ssn,
                            PhoneNumber = newPhoneNumber
                        });

                    var user = await _userManager.FindByNameAsync(ssn);
                    if (user != null)
                    {
                        user.PhoneNumber = newPhoneNumber;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated Successfully" });
        }

        [HttpPost("add-course")]
        public async Task<ActionResult> AddCourse([FromBody] CourseDto model)
        {
            // ✅ Check لو الكورس موجود بالفعل
            var existingCourse = await _context.Courses.FindAsync(model.CourseId);
            if (existingCourse != null)
                return BadRequest("Course already exists");

            // ✅ Check لو الـ AdminSSN موجود
            var admin = await _context.Adminstrations.FindAsync(model.AdminSsn);
            if (admin == null)
                return BadRequest("Admin SSN not found");

            // ✅ Check لو الـ Level موجود
            var level = await _context.Levels.FindAsync(model.CourseLevel);
            if (level == null)
                return BadRequest("Level not found");

            // ✅ Check لو الـ PreReqCourse موجود لو اتبعت
            if (model.PreReqCourseId != null)
            {
                var preReq = await _context.Courses.FindAsync(model.PreReqCourseId);
                if (preReq == null)
                    return BadRequest("Pre-requisite course not found");
            }

            var course = new Course
            {
                CourseId = model.CourseId,
                CourseName = model.CourseName,
                CourseSemester = model.CourseSemester,
                DepartmentName = model.DepartmentName,
                AdminSsn = model.AdminSsn,
                CourseLevel = model.CourseLevel,
                PreReqCourseId = model.PreReqCourseId,
                CreditHours = model.CreditHours
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Course added successfully" });
        }

        [HttpPut("edit-course/{courseId}")]
        public async Task<ActionResult> EditCourse(string CourseId, [FromBody] CourseDto model)
        {
            var course = await _context.Courses.FindAsync(CourseId);
            if (course == null) return NotFound("Course not found");

            // ✅ Check لو الـ AdminSSN موجود
            if (model.AdminSsn != null && model.AdminSsn != course.AdminSsn)
            {
                var admin = await _context.Adminstrations.FindAsync(model.AdminSsn);
                if (admin == null)
                    return BadRequest("Admin SSN not found");
            }

            // ✅ Check لو الـ Level موجود
            if (model.CourseLevel != 0)
            {
                var level = await _context.Levels.FindAsync(model.CourseLevel);
                if (level == null)
                    return BadRequest("Level not found");
            }

            // ✅ Check لو الـ PreReqCourse موجود
            if (model.PreReqCourseId != null)
            {
                var preReq = await _context.Courses.FindAsync(model.PreReqCourseId);
                if (preReq == null)
                    return BadRequest("Pre-requisite course not found");
            }

            course.CourseName = model.CourseName ?? course.CourseName;
            course.CourseSemester = model.CourseSemester ?? course.CourseSemester;
            course.DepartmentName = model.DepartmentName ?? course.DepartmentName;
            course.AdminSsn = model.AdminSsn ?? course.AdminSsn;
            course.CourseLevel = model.CourseLevel != 0 ? model.CourseLevel : course.CourseLevel;
            course.PreReqCourseId = model.PreReqCourseId ?? course.PreReqCourseId;
            course.CreditHours = model.CreditHours != 0 ? model.CreditHours : course.CreditHours;

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Course updated successfully" });
        }

        [HttpDelete("delete-course/{courseId}")]
        public async Task<ActionResult> DeleteCourse(string courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound("Course not found");

            // ✅ امسح الـ Enrollments المرتبطة بالكورس الأول
            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .ToListAsync();
            if (enrollments.Any())
                _context.Enrollments.RemoveRange(enrollments);

            //// ✅ امسح الـ SelectedCourses المرتبطة بالكورس
            //var selectedCourses = await _context.Set<Dictionary<string, object>>("SelectedCourses")
            //    .Where(sc => EF.Property<string>(sc, "CourseId") == courseId)
            //    .ToListAsync();
            //if (selectedCourses.Any())
            //    _context.Set<Dictionary<string, object>>("SelectedCourses").RemoveRange(selectedCourses);

            // ✅ امسح الكورسات اللي بتاخد الكورس ده كـ PreReq
            var dependentCourses = await _context.Courses
                .Where(c => c.PreReqCourseId == courseId)
                .ToListAsync();
            if (dependentCourses.Any())
            {
                foreach (var c in dependentCourses)
                    c.PreReqCourseId = null;
            }

            // ✅ امسح الكورس
            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Course deleted successfully" });
        }
    }
}
