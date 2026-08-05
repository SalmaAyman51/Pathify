using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pathify.DTOs;
using Pathify.Models;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using static Pathify.DTOs.CourseDTO;
using static Pathify.DTOs.CreateExternalProfessorDTO;
using static Pathify.DTOs.CreateInternalProfessorDTO;
using static Pathify.DTOs.EnrollmentEditDTO;
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

        [HttpGet("pending-approvals")]
        public async Task<ActionResult> GetPendingApprovals()
        {
            var pendingStudents = await _context.TempStudentData
            .Where(t => t.UserId != null) // ✅ بس اللي عملوا تسجيل فعلي
             .Select(t => new
             {
                    t.SSN,
                    t.FirstName,
                    t.LastName,
                    t.Email,
                    t.StudentId,
                    t.AcademicLevel,
                    t.GPA,
                    t.EnrollmentYear,
                    t.Gender,
                    t.BirthDate,
                    t.LevelId,
                    t.ProjectId,
                    t.TeamId,
                    t.PhoneNumber
             })
                .ToListAsync();

            if (!pendingStudents.Any())
                return NotFound("No pending approvals");

            return Ok(pendingStudents);
        }

        [HttpPost("approve-user/{email}")]
        public async Task<IActionResult> ApproveUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found");

            var tempStudent = await _context.TempStudentData
                .FirstOrDefaultAsync(t => t.SSN == user.SSN);
            if (tempStudent == null) return NotFound("Student data not found");

            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == tempStudent.SSN
                                        || s.StudentId == tempStudent.StudentId);
            if (existingStudent != null)
                return BadRequest("Student already exists");

            user.IsApproved = true;
            await _userManager.UpdateAsync(user);

            var student = new Student
            {
                StudentSsn = tempStudent.SSN,
                StudentId = tempStudent.StudentId,
                Fname = tempStudent.FirstName,
                Lname = tempStudent.LastName,
                Email = tempStudent.Email,
                BirthDate = DateOnly.FromDateTime(tempStudent.BirthDate),
                Gender = tempStudent.Gender ?? "N/A",
                EnrollmentYear = tempStudent.EnrollmentYear,
                Gpa = (decimal?)tempStudent.GPA,
                AcademicLevel = tempStudent.AcademicLevel,
                LevelId = tempStudent.LevelId ?? 1,
                TeamId = tempStudent.TeamId,
                ProjectId = tempStudent.ProjectId,
                PhoneNumber = tempStudent.PhoneNumber,
                IsApproved = true
            };

            _context.Students.Add(student);
            _context.TempStudentData.Remove(tempStudent);
            await _context.SaveChangesAsync();

            return Ok("User approved successfully");
        }

        [HttpDelete("reject-user/{email}")]
        public async Task<IActionResult> RejectUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found");

            var tempStudent = await _context.TempStudentData
                .FirstOrDefaultAsync(t => t.SSN == user.SSN);

            if (tempStudent != null)
                _context.TempStudentData.Remove(tempStudent);

            await _userManager.DeleteAsync(user);
            await _context.SaveChangesAsync();

            return Ok("User rejected and removed successfully");
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
            await _context.SaveChangesAsync();
            return Ok(new { message = "Edit is done" });
        }
        [HttpGet("get-student/{SSN}")]
        public async Task<ActionResult> GetStudent(string SSN)
        {
            var student = await _context.Students.FindAsync(SSN);
            if (student == null) return NotFound("Student not Found");

            return Ok(new
            {
                student.StudentSsn,
                student.StudentId,
                student.Fname,
                student.Lname,
                student.FullName,
                student.Email,
                student.Gpa,
                student.AcademicLevel,
                student.LevelId,
                student.Gender,
                student.BirthDate,
                student.EnrollmentYear,
                student.TeamId,
                student.ProjectId,
            });
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

        [HttpPost("add-internal-professor")]
        public async Task<IActionResult> AddInternalProfessor([FromBody] CreateInternalProfessorDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.SSN))
                return BadRequest("SSN is required");

            if (string.IsNullOrWhiteSpace(model.FullName))
                return BadRequest("Full Name is required");

            if (string.IsNullOrWhiteSpace(model.DeptName))
                return BadRequest("Department Name is required");

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                return BadRequest("Phone Number is required");

            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest("Email is required");

            var exists = await _context.InternalProfessors
                .AnyAsync(p => p.InternalProfessorSsn == model.SSN);
            if (exists) return BadRequest("Professor already exists");

            var professor = new InternalProfessor
            {
                InternalProfessorSsn = model.SSN,
                InternalProfessorName = model.FullName,
                DeptName = model.DeptName,
                Email = model.Email
            };
            _context.InternalProfessors.Add(professor);
            await _context.SaveChangesAsync();

            var phone = new InternalProfessorPhone
            {
                InternalProfessorSsn = model.SSN,
                PhoneNumber = model.PhoneNumber
            };
            _context.InternalProfessorPhones.Add(phone);
            await _context.SaveChangesAsync();

            return Ok("Internal Professor added successfully");
        }

        // ✅ إضافة External Professor
        [HttpPost("add-external-professor")]
        public async Task<IActionResult> AddExternalProfessor([FromBody] CreateExternalProfessorDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.SSN))
                return BadRequest("SSN is required");

            if (string.IsNullOrWhiteSpace(model.FullName))
                return BadRequest("Full Name is required");

            if (string.IsNullOrWhiteSpace(model.DeptName))
                return BadRequest("Department Name is required");

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                return BadRequest("Phone Number is required");

            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest("Email is required");

            var exists = await _context.ExternalProfessors
                .AnyAsync(p => p.ExternalProfessorSsn == model.SSN);
            if (exists) return BadRequest("Professor already exists");

            var professor = new ExternalProfessor
            {
                ExternalProfessorSsn = model.SSN,
                ExternalProfessorName = model.FullName,
                DeptName = model.DeptName,
                Email = model.Email
            };
            _context.ExternalProfessors.Add(professor);
            await _context.SaveChangesAsync();

            var phone = new ExternalProfessorPhone
            {
                ExternalProfessorSsn = model.SSN,
                PhoneNumber = model.PhoneNumber
            };
            _context.ExternalProfessorPhones.Add(phone);
            await _context.SaveChangesAsync();

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
                Phone = i.InternalProfessorPhones.FirstOrDefault()?.PhoneNumber ?? "N/A",
                Type = "Internal"   // ← ضيف دي
            }).Concat(externals.Select(e => new {
                Id = e.ExternalProfessorSsn,
                Name = e.ExternalProfessorName,
                Dept = e.DeptName ?? "",
                Phone = e.ExternalProfessorPhones.FirstOrDefault()?.PhoneNumber ?? "N/A",
                Type = "External"   // ← وضيف دي
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
                CreditHours = model.CreditHours,
                CourseType=model.CourseType
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

            // ✅ امسح الكورسات اللي بتاخد الكورس ده كـ PreReq
            var dependentCourses = await _context.Courses
                .Where(c => c.PreReqCourseId == courseId)
                .ToListAsync();
            if (dependentCourses.Any())
            {
                foreach (var c in dependentCourses)
                    c.PreReqCourseId = null;
            }

           
            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Course deleted successfully" });
        }
        [HttpGet("get-course/{courseId}")]
        public async Task<ActionResult> GetCourse(string courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound("Course not found");

            return Ok(new
            {
                course.CourseId,
                course.CourseName,
                course.CourseSemester,
                course.DepartmentName,
                course.AdminSsn,
                course.CourseLevel,
                course.PreReqCourseId,
                course.CreditHours,
                course.CourseType,
            });
        }
        [HttpGet("get-all-courses")]
        public async Task<ActionResult> GetAllCourses()
        {
            var courses = await _context.Courses
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseName,
                    c.CourseSemester,                  
                    c.CourseLevel,                  
                    c.PreReqCourseId,
                    c.CourseType
                })
                .ToListAsync();

            if (!courses.Any())
                return NotFound("No courses found");

            return Ok(courses);
        }

        [HttpPut("update-semester")]
        public async Task<IActionResult> UpdateAllStudentsSemester([FromBody] string semester)
        {
            var students = await _context.Students.ToListAsync();
            foreach (var s in students)
                s.CurrentSemester = semester;

            await _context.SaveChangesAsync();
            return Ok("Semester updated for all students");
        }

        [HttpPut("edit-enrollment/{ssn}")]
        public async Task<ActionResult> EditEnrollment(string ssn, [FromBody] EnrollmentEditDto model)
        {
            // ✅ جيب الـ Enrollment الحالي بالـ OldCourseId
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentSsn == ssn && e.CourseId == model.OldCourseId);
            if (enrollment == null) return NotFound("Enrollment not found");

            // ✅ تأكد إن الكورس الجديد موجود
            var newCourse = await _context.Courses.FindAsync(model.NewCourseId);
            if (newCourse == null) return BadRequest("New course not found");

            // ✅ تأكد إن الطالب مش مسجل في الكورس الجديد بالفعل
            var alreadyEnrolled = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentSsn == ssn && e.CourseId == model.NewCourseId);
            if (alreadyEnrolled != null)
                return BadRequest("Student already enrolled in this course");

            // ✅ تأكد إن الطالب محقق الـ Prerequisite
            if (newCourse.PreReqCourseId != null)
            {
                var passedPreReq = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentSsn == ssn
                                           && e.CourseId == newCourse.PreReqCourseId
                                           && e.Passed == PassStatus.Passed);
                if (passedPreReq == null)
                    return BadRequest($"Student must pass course '{newCourse.PreReqCourseId}' before enrolling in '{model.NewCourseId}'");
            }

            _context.Enrollments.Remove(enrollment);

            var newEnrollment = new Enrollment
            {
                StudentSsn = ssn,
                CourseId = model.NewCourseId,
                EnrollmentDate = enrollment.EnrollmentDate,
                AdminSsn = enrollment.AdminSsn,
                Passed = enrollment.Passed
            };

            _context.Enrollments.Add(newEnrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrollment updated successfully" });
        }

        [HttpDelete("delete-enrollment/{ssn}/{courseId}")]
        public async Task<ActionResult> DeleteEnrollment(string ssn, string courseId)
        {
            // ✅ تأكد إن الطالب موجود
            var student = await _context.Students.FindAsync(ssn);
            if (student == null) return NotFound("Student not found");

            // ✅ جيب الـ Enrollment
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentSsn == ssn && e.CourseId == courseId);
            if (enrollment == null) return NotFound("Enrollment not found");

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrollment deleted successfully" });
        }

        [HttpGet("get-all-enrollments")]
        public async Task<ActionResult> GetAllEnrollments()
        {
            var enrollments = await _context.Enrollments
                .Select(e => new
                {
                    e.StudentSsn,
                    e.CourseId,
                    CourseName = e.Course.CourseName,
                    e.EnrollmentDate,
                    e.Passed,
                    e.AdminSsn
                })
                .ToListAsync();

            if (!enrollments.Any())
                return NotFound("No enrollments found");

            return Ok(enrollments);
        }
        [HttpGet("get-students-count")]
        public async Task<ActionResult> GetStudentsCount()
        {
            var count = await _context.Students.CountAsync();
            return Ok(new { studentsCount = count });
        }

        [HttpGet("get-professors-count")]
        public async Task<ActionResult> GetProfessorsCount()
        {
            var totalCount = await _context.InternalProfessors.CountAsync()
                           + await _context.ExternalProfessors.CountAsync();

            return Ok(new { totalProfessorsCount = totalCount });
        }





        [HttpGet("get-pending-approvals-count")]
        public async Task<ActionResult> GetPendingApprovalsCount()
        {
            var count = await _context.TempStudentData
                .Where(t => _context.Users.Any(u => u.SSN == t.SSN))
                .CountAsync();

            return Ok(new { pendingApprovalsCount = count });
        }
        [HttpGet("get-active-courses-count")]
        public async Task<ActionResult> GetActiveCoursesCount()
        {
            var count = await _context.Courses
                .CountAsync(c => c.Enrollments.Any());

            return Ok(new { activeCoursesCount = count });
        }

        [HttpGet("search-students/{name}")]
        public async Task<ActionResult> SearchStudents(string name)
        {
            var students = await _context.Students
                .Where(s => s.Fname.StartsWith(name) || s.Lname.StartsWith(name))
                .Select(s => new
                {
                    s.StudentSsn,
                    s.StudentId,
                    s.Fname,
                    s.Lname,
                    s.Email,
                    s.AcademicLevel,
                    s.Gpa
                })
                .ToListAsync();

            if (!students.Any())
                return NotFound("No students found");

            return Ok(students);
        }

        [HttpGet("search-professors/{name}")]
        public async Task<ActionResult> SearchProfessors(string name)
        {
            var internalProfs = await _context.InternalProfessors
                .Where(p => p.InternalProfessorName.StartsWith(name))
                .Select(p => new
                {
                    p.InternalProfessorSsn,
                    p.InternalProfessorName,
                    p.DeptName,
                    Type = "Internal"
                })
                .ToListAsync();

            var externalProfs = await _context.ExternalProfessors
                .Where(p => p.ExternalProfessorName.StartsWith(name))
                .Select(p => new
                {
                    p.ExternalProfessorSsn,
                    p.ExternalProfessorName,
                    p.DeptName,
                    Type = "External"
                })
                .ToListAsync();

            var result = internalProfs.Cast<object>().Concat(externalProfs).ToList();

            if (!result.Any())
                return NotFound("No professors found");

            return Ok(result);
        }

        [HttpGet("search-courses/{query}")]
        public async Task<ActionResult> SearchCourses(string query)
        {
            var courses = await _context.Courses
                .Where(c => c.CourseId.StartsWith(query) || c.CourseName.StartsWith(query))
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseName,
                    c.CourseSemester,
                    c.DepartmentName,
                    c.CourseLevel,
                    c.CreditHours,
                    c.CourseType
                })
                .ToListAsync();

            if (!courses.Any())
                return NotFound("No courses found");

            return Ok(courses);
        }
        [HttpGet("search-enrollment/{query}")]
        public async Task<ActionResult> SearchEnrollment(string query)
        {
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentSsn.StartsWith(query) ||
                            e.StudentSsnNavigation.Fname.StartsWith(query) ||
                            e.StudentSsnNavigation.Lname.StartsWith(query))
                .Select(e => new
                {
                    e.StudentSsn,
                    StudentName = e.StudentSsnNavigation.Fname + " " + e.StudentSsnNavigation.Lname,
                    e.CourseId,
                    CourseName = e.Course.CourseName,
                    e.EnrollmentDate,
                    e.Passed
                })
                .ToListAsync();

            if (!enrollments.Any())
                return NotFound("No enrollments found");

            return Ok(enrollments);
        }
//------------------------------------------------------------------------------------------------
        [HttpPut("set-team-members-limit/{min}/{max}")]
        public async Task<ActionResult> SetTeamMembersLimit(int min, int max)
        {
            if (min <= 0 || max <= 0)
                return BadRequest("Min and Max members must be positive numbers");

            if (min >= max)
                return BadRequest("Min members must be less than Max members");

            // ✅ تأكد إنه مفيش تيمات مسجلة
            var anyTeam = await _context.Teams.AnyAsync();
            if (anyTeam)
                return BadRequest("Cannot change limits after teams have been registered");

            // ✅ تأكد إنه مش متحدد قبل كده
            var existingLimit = await _context.TeamLimits.FirstOrDefaultAsync();
            if (existingLimit != null)
                return BadRequest("Team limits have already been set");

            _context.TeamLimits.Add(new TeamLimit
            {
                MinMembers = min,
                MaxMembers = max
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Team size set: Min = {min}, Max = {max}" });
        }
        //---------------------------------------------------------------------------------------------
        [HttpPost("import-students")]
        public async Task<IActionResult> ImportStudents(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid Excel file.");

            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only Excel files (.xlsx, .xls) are allowed.");

            var importedStudents = new List<TempStudentData>();
            var errors = new List<string>();
            int rowNumber = 1; // عشان نتتبع رقم الصف في رسائل الأخطاء

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1); // أول شيت
                    var rows = worksheet.RowsUsed().Skip(1); // ✅ نتخطى صف الـ Header

                    foreach (var row in rows)
                    {
                        rowNumber++;

                        try
                        {
                            var ssn = row.Cell(1).GetString().Trim();
                            var firstName = row.Cell(2).GetString().Trim();
                            var lastName = row.Cell(3).GetString().Trim();
                            var studentId = row.Cell(4).GetValue<int>();
                            var email = row.Cell(5).GetString().Trim();
                            var birthDateCell = row.Cell(6);
                            var gender = row.Cell(7).GetString().Trim();
                            var enrollmentYear = row.Cell(8).GetValue<int?>();
                            var gpa = row.Cell(9).GetValue<double?>();
                            var academicLevel = row.Cell(10).GetString().Trim();
                            var levelId = row.Cell(11).GetValue<int?>();
                            var projectId = row.Cell(12).GetValue<int?>();
                            var teamId = row.Cell(13).GetValue<int?>();
                            var currentSemester = row.Cell(14).GetString().Trim();
                            var phoneNumber = row.Cell(15).GetString().Trim();

                            // ✅ Validation أساسي
                            if (string.IsNullOrWhiteSpace(ssn) || ssn.Length != 14)
                            {
                                errors.Add($"Row {rowNumber}: Invalid or missing SSN.");
                                continue;
                            }

                            // ✅ Check التكرار جوه نفس الملف
                            if (importedStudents.Any(s => s.SSN == ssn))
                            {
                                errors.Add($"Row {rowNumber}: Duplicate SSN ({ssn}) within the file.");
                                continue;
                            }

                            // ✅ Check لو موجود بالفعل في Students أو Temp
                            var existsInStudents = await _context.Students
                                .AnyAsync(s => s.StudentSsn == ssn || s.StudentId == studentId || s.Email == email);

                            var existsInTemp = await _context.TempStudentData
                                .AnyAsync(t => t.SSN == ssn || t.StudentId == studentId || t.Email == email);

                            if (existsInStudents || existsInTemp)
                            {
                                errors.Add($"Row {rowNumber}: Student with SSN {ssn} already exists.");
                                continue;
                            }

                            //// ✅ تحويل تاريخ الميلاد بأمان
                            //DateTime birthDate;
                            //if (birthDateCell.DataType == XLDataType.DateTime)
                            //{
                            //    birthDate = birthDateCell.GetDateTime();
                            //}
                            //else if (!DateTime.TryParse(birthDateCell.GetString(), out birthDate))
                            //{
                            //    errors.Add($"Row {rowNumber}: Invalid BirthDate format.");
                            //    continue;
                            //}
                            DateTime birthDate;

                            if (birthDateCell.DataType == XLDataType.DateTime)
                            {
                                birthDate = birthDateCell.GetDateTime();
                            }
                            else if (birthDateCell.DataType == XLDataType.Number)
                            {
                                // الإكسل بيخزن التاريخ كـ Serial Number أحيانًا لو الخلية مش متفرمتة كـ Date
                                birthDate = DateTime.FromOADate(birthDateCell.GetDouble());
                            }
                            else
                            {
                                var rawValue = birthDateCell.GetString().Trim();
                                string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "M/d/yyyy" };

                                if (!DateTime.TryParseExact(rawValue, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate)
                                    && !DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate))
                                {
                                    errors.Add($"Row {rowNumber}: Invalid BirthDate format. Value: '{rawValue}'");
                                    continue;
                                }
                            }

                            var tempStudent = new TempStudentData
                            {
                                SSN = ssn,
                                FirstName = firstName,
                                LastName = lastName,
                                StudentId = studentId,
                                Email = email,
                                BirthDate = birthDate,
                                Gender = gender,
                                EnrollmentYear = enrollmentYear ?? 0,
                                GPA = gpa ?? 0,
                                AcademicLevel = academicLevel,
                                LevelId = levelId,
                                ProjectId = projectId,
                                TeamId = teamId,
                                CurrentSemester = currentSemester,
                                PhoneNumber = phoneNumber
                            };

                            importedStudents.Add(tempStudent);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Row {rowNumber}: {ex.Message}");
                        }
                    }
                }
            }

            if (importedStudents.Any())
            {
                _context.TempStudentData.AddRange(importedStudents);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = $"{importedStudents.Count} students imported successfully.",
                importedCount = importedStudents.Count,
                errors = errors
            });
        }

        // ===== Recent Enrollment Endpoint =====
        [HttpGet("recent-enrollment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRecentEnrollment()
        {
            var recent = await _context.Enrollments
                .Include(e => e.StudentSsnNavigation)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrollmentDate)
                .FirstOrDefaultAsync();

            if (recent == null)
                return NotFound(new { message = "No enrollments found." });

            var result = new
            {
                studentName = recent.StudentSsnNavigation.FullName,
                courseName = recent.Course.CourseName,
                courseLevel = recent.Course.CourseLevel,
                courseSemester = recent.Course.CourseSemester,
                enrollmentDate = recent.EnrollmentDate
            };

            return Ok(result);
        }

        // ===== System Health Endpoint =====
        [HttpGet("system-health")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSystemHealth()
        {
            bool dbHealthy;
            try
            {
                dbHealthy = await _context.Database.CanConnectAsync();
            }
            catch
            {
                dbHealthy = false;
            }

            // Auth Service: لو السطر ده اتنفذ معنى ان الـ JWT/middleware شغالة فعليًا
            var authActive = true;

            // Load تقريبي مبني على استهلاك الـ process الحالي (Working Set) مقابل حد أقصى منطقي (مثلاً 1GB)
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var workingSetMb = process.WorkingSet64 / (1024.0 * 1024.0);
            var loadPercentage = Math.Min(100, Math.Round((workingSetMb / 1024.0) * 100, 0));

            var result = new
            {
                database = dbHealthy ? "Healthy" : "Down",
                authService = authActive ? "Active" : "Inactive",
                loadPercentage = (int)loadPercentage
            };

            return Ok(result);
        }

        [HttpGet("Profile")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProfile()
        {
            var ssn = User.FindFirstValue("SSN") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(ssn))
                return Unauthorized(new { message = "SSN claim not found in token." });

            var admin = await _context.Adminstrations
                .Include(a => a.AdminPhones)
                .FirstOrDefaultAsync(a => a.AdminSsn == ssn);

            if (admin == null)
                return NotFound(new { message = "Admin not found." });

            var result = new
            {
                AdminSsn = admin.AdminSsn,
                Fname = admin.Fname,
                Lname = admin.Lname,
                FullName = admin.FullName,
                AdminPhones = admin.AdminPhones.Select(p => new { Phone = p.PhoneNumber }).ToList()
            };

            return Ok(result);
        }

        // PUT: api/Admin/Profile
        [HttpPut("Profile")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateAdminProfileDto dto)
        {
            var ssn = User.FindFirstValue("SSN") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(ssn))
                return Unauthorized(new { message = "SSN claim not found in token." });

            var admin = await _context.Adminstrations
                .Include(a => a.AdminPhones)
                .FirstOrDefaultAsync(a => a.AdminSsn == ssn);

            if (admin == null)
                return NotFound(new { message = "Admin not found." });

            if (!string.IsNullOrWhiteSpace(dto.Fname))
                admin.Fname = dto.Fname;

            if (!string.IsNullOrWhiteSpace(dto.Lname))
                admin.Lname = dto.Lname;

            // تحديث الاسم الكامل تلقائياً عند تغيير الاسم الأول أو الأخير
            admin.FullName = $"{admin.Fname} {admin.Lname}".Trim();

            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var existingPhone = admin.AdminPhones.FirstOrDefault();

                if (existingPhone != null)
                {
                    existingPhone.PhoneNumber = dto.Phone;
                }
                else
                {
                    admin.AdminPhones.Add(new AdminPhone
                    {
                        AdminSsn = admin.AdminSsn,
                        PhoneNumber = dto.Phone
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }


        [HttpPost("import-professors")]
        public async Task<IActionResult> ImportProfessors(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid Excel file.");

            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only Excel files (.xlsx, .xls) are allowed.");

            var importedInternal = new List<InternalProfessor>();
            var importedExternal = new List<ExternalProfessor>();
            var importedInternalSsns = new HashSet<string>();
            var importedExternalSsns = new HashSet<string>();
            var errors = new List<string>();
            int rowNumber = 1; // عشان نتتبع رقم الصف في رسائل الأخطاء

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1); // أول شيت
                    var rows = worksheet.RowsUsed().Skip(1); // ✅ نتخطى صف الـ Header

                    foreach (var row in rows)
                    {
                        rowNumber++;

                        try
                        {
                            var type = row.Cell(1).GetString().Trim();
                            var ssn = row.Cell(2).GetString().Trim();
                            var name = row.Cell(3).GetString().Trim();
                            var deptOrCompany = row.Cell(4).GetString().Trim();
                            var email = row.Cell(5).GetString().Trim();
                            var phoneNumber = row.Cell(6).GetString().Trim();

                            // ✅ Validation أساسي
                            if (string.IsNullOrWhiteSpace(ssn) || ssn.Length != 14)
                            {
                                errors.Add($"Row {rowNumber}: Invalid or missing SSN.");
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(name))
                            {
                                errors.Add($"Row {rowNumber}: Name is required.");
                                continue;
                            }

                            bool isInternal = type.Equals("Internal", StringComparison.OrdinalIgnoreCase);
                            bool isExternal = type.Equals("External", StringComparison.OrdinalIgnoreCase);

                            if (!isInternal && !isExternal)
                            {
                                errors.Add($"Row {rowNumber}: Type must be 'Internal' or 'External'. Value: '{type}'.");
                                continue;
                            }

                            // ✅ Check التكرار جوه نفس الملف
                            if (importedInternalSsns.Contains(ssn) || importedExternalSsns.Contains(ssn))
                            {
                                errors.Add($"Row {rowNumber}: Duplicate SSN ({ssn}) within the file.");
                                continue;
                            }

                            if (isInternal)
                            {
                                // ✅ Check لو موجود بالفعل
                                var existsInternal = await _context.InternalProfessors
                                    .AnyAsync(p => p.InternalProfessorSsn == ssn || p.Email == email);
                                var existsExternal = await _context.ExternalProfessors
                                    .AnyAsync(p => p.ExternalProfessorSsn == ssn || p.Email == email);

                                if (existsInternal || existsExternal)
                                {
                                    errors.Add($"Row {rowNumber}: Professor with SSN {ssn} already exists.");
                                    continue;
                                }

                                if (string.IsNullOrWhiteSpace(deptOrCompany))
                                {
                                    errors.Add($"Row {rowNumber}: DeptName is required for internal professors.");
                                    continue;
                                }

                                var internalProf = new InternalProfessor
                                {
                                    InternalProfessorSsn = ssn,
                                    InternalProfessorName = name,
                                    DeptName = deptOrCompany,
                                    Email = email,
                                    InternalProfessorPhones = string.IsNullOrWhiteSpace(phoneNumber)
                                        ? new List<InternalProfessorPhone>()
                                        : new List<InternalProfessorPhone>
                                        {
                                    new InternalProfessorPhone
                                    {
                                        InternalProfessorSsn = ssn,
                                        PhoneNumber = phoneNumber
                                    }
                                        }
                                };

                                importedInternal.Add(internalProf);
                                importedInternalSsns.Add(ssn);
                            }
                            else
                            {
                                var existsInternal = await _context.InternalProfessors
                                    .AnyAsync(p => p.InternalProfessorSsn == ssn || p.Email == email);
                                var existsExternal = await _context.ExternalProfessors
                                    .AnyAsync(p => p.ExternalProfessorSsn == ssn || p.Email == email);

                                if (existsInternal || existsExternal)
                                {
                                    errors.Add($"Row {rowNumber}: Professor with SSN {ssn} already exists.");
                                    continue;
                                }

                                var externalProf = new ExternalProfessor
                                {
                                    ExternalProfessorSsn = ssn,
                                    ExternalProfessorName = name,
                                    DeptName = deptOrCompany,
                                    // عدّل اسم الـ property ده لو مختلف عندك (مثلاً CompanyName)
                                    Email = email,
                                    ExternalProfessorPhones = string.IsNullOrWhiteSpace(phoneNumber)
                                        ? new List<ExternalProfessorPhone>()
                                        : new List<ExternalProfessorPhone>
                                        {
                                    new ExternalProfessorPhone
                                    {
                                        ExternalProfessorSsn = ssn,
                                        PhoneNumber = phoneNumber
                                    }
                                        }
                                };

                                importedExternal.Add(externalProf);
                                importedExternalSsns.Add(ssn);
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Row {rowNumber}: {ex.Message}");
                        }
                    }
                }
            }
             
            if (importedInternal.Any())
                _context.InternalProfessors.AddRange(importedInternal);

            if (importedExternal.Any())
                _context.ExternalProfessors.AddRange(importedExternal);

            if (importedInternal.Any() || importedExternal.Any())
                await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{importedInternal.Count + importedExternal.Count} professors imported successfully.",
                importedInternalCount = importedInternal.Count,
                importedExternalCount = importedExternal.Count,
                errors = errors
            });
        }


        [HttpPost("past-projects/upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadPastYearsProjects(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var projectsToAdd = new List<PastYearsProjects>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1); // skip header row

                    int rowNumber = 1; // for error messages (header = row 1)
                    foreach (var row in rows)
                    {
                        rowNumber++;

                        var title = row.Cell(1).GetValue<string>();
                        var description = row.Cell(2).GetValue<string>();
                        var yearCell = row.Cell(3);

                        if (string.IsNullOrWhiteSpace(title))
                            return BadRequest($"Row {rowNumber}: Title is required.");

                        if (string.IsNullOrWhiteSpace(description))
                            return BadRequest($"Row {rowNumber}: Description is required.");

                        if (yearCell.IsEmpty() || !yearCell.TryGetValue<int>(out int year))
                            return BadRequest($"Row {rowNumber}: Year must be a valid number.");

                        projectsToAdd.Add(new PastYearsProjects
                        {
                            Title = title,
                            Description = description,
                            Year = year
                        });
                    }
                }
            }

            if (projectsToAdd.Count == 0)
                return BadRequest("No valid rows found in the file.");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.PastYearsProjects.AddRange(projectsToAdd);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Failed to save projects: {ex.Message}");
                }
            }

            return Ok(new { message = $"{projectsToAdd.Count} projects uploaded successfully." });
        }


        [HttpPost("import-results")]
        public async Task<IActionResult> ImportResults(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid Excel file.");

            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only Excel files (.xlsx, .xls) are allowed.");

            var errors = new List<string>();
            int updated = 0;
            int rowNumber = 1; // عشان نتتبع رقم الصف في رسائل الأخطاء

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1); // أول شيت
                    var rows = worksheet.RowsUsed().Skip(1); // ✅ نتخطى صف الـ Header

                    foreach (var row in rows)
                    {
                        rowNumber++;

                        try
                        {
                            var ssn = row.Cell(1).GetString().Trim();
                            var courseId = row.Cell(2).GetString().Trim();
                            var resultText = row.Cell(3).GetString().Trim();

                            if (string.IsNullOrWhiteSpace(ssn) || string.IsNullOrWhiteSpace(courseId))
                            {
                                errors.Add($"Row {rowNumber}: SSN and CourseId are required.");
                                continue;
                            }

                            bool isPassed = resultText.Equals("Passed", StringComparison.OrdinalIgnoreCase);
                            bool isFailed = resultText.Equals("Failed", StringComparison.OrdinalIgnoreCase);

                            if (!isPassed && !isFailed)
                            {
                                errors.Add($"Row {rowNumber}: Result must be 'Passed' or 'Failed'. Value: '{resultText}'.");
                                continue;
                            }

                            var enrollment = await _context.Enrollments
                                .FirstOrDefaultAsync(e => e.StudentSsn == ssn && e.CourseId == courseId);

                            if (enrollment == null)
                            {
                                errors.Add($"Row {rowNumber}: No enrollment found for SSN {ssn} in course {courseId}.");
                                continue;
                            }

                            enrollment.Passed = isPassed ? PassStatus.Passed : PassStatus.Failed;
                            updated++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Row {rowNumber}: {ex.Message}");
                        }
                    }
                }
            }

            if (updated > 0)
                await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{updated} enrollment(s) updated successfully.",
                updated,
                errors
            });
        }
    }

}


