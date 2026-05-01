using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pathify.Models;
using System.Security.Claims;
using static Pathify.DTOs.AddCourseRequestDTO;
using static Pathify.DTOs.EnrollCoursesDTO;

namespace Pathify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class CourseEnrollmentController : ControllerBase
    {
        private readonly PathifyContext _context;

        public CourseEnrollmentController(PathifyContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Student")]
        [HttpGet("available-courses")]
        public async Task<IActionResult> GetAvailableCourses()
        {
            var studentSSN = User.FindFirstValue("SSN");
            if (studentSSN == null)
                return Unauthorized("Invalid token");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == studentSSN);

            if (student == null)
                return NotFound("Student not found");

            var passedCourses = await _context.Enrollments
                .Where(e => e.StudentSsn == studentSSN && e.Passed == true)
                .Select(e => e.CourseId)
                .ToListAsync();

            // 👇 السنة الحالية
            var currentYear = DateTime.Now.Year;

            // 👇 الكورسات اللي الطالب already enrolled فيها في نفس السنة
            var enrolledThisYear = await _context.Enrollments
     .Where(e => e.StudentSsn == studentSSN &&
                 e.Passed == null &&
                 e.EnrollmentDate.HasValue &&
                 e.EnrollmentDate.Value.Year == currentYear)
     .Select(e => e.CourseId)
     .ToListAsync();

            string currentSemester = (student.CurrentSemester ?? "first semester").ToLower();
            int currentLevel = student.LevelId ?? 1;

            int maxCourses;
            if (currentLevel == 1 || student.Gpa == 0 || student.Gpa == null)
                maxCourses = 6;
            else if (student.Gpa >= 2)
                maxCourses = 6;
            else
                maxCourses = 4;

            var allCourses = await _context.Courses.ToListAsync();

            // ✅ كورسات المستوى الحالي
            var currentLevelCourses = allCourses
                .Where(c =>
                    c.CourseLevel == currentLevel &&
                    c.CourseSemester.ToLower() == currentSemester &&
                    !passedCourses.Contains(c.CourseId) &&
                    !enrolledThisYear.Contains(c.CourseId) && // 👈 المهم
                    (string.IsNullOrEmpty(c.PreReqCourseId) ||
                     passedCourses.Contains(c.PreReqCourseId))
                )
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseName,
                    c.CourseLevel,
                    c.CourseSemester,
                    c.CreditHours,
                    c.DepartmentName,
                    c.CourseType,
                    PreRequisite = c.PreReqCourseId,
                    HasPrerequisite = !string.IsNullOrEmpty(c.PreReqCourseId),
                    Source = "Current Level"
                });

            // ✅ المستوى الأعلى
            var higherLevelCourses = allCourses
                .Where(c =>
                    c.CourseLevel > currentLevel &&
                    c.CourseSemester.ToLower() == currentSemester &&
                    !passedCourses.Contains(c.CourseId) &&
                    !enrolledThisYear.Contains(c.CourseId) && // 👈 المهم
                    (string.IsNullOrEmpty(c.PreReqCourseId) ||
                     passedCourses.Contains(c.PreReqCourseId))
                )
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseName,
                    c.CourseLevel,
                    c.CourseSemester,
                    c.CreditHours,
                    c.DepartmentName,
                    c.CourseType,
                    PreRequisite = c.PreReqCourseId,
                    HasPrerequisite = !string.IsNullOrEmpty(c.PreReqCourseId),
                    Source = "Higher Level"
                });

            // ✅ المستوى الأقل
            var lowerLevelCourses = allCourses
                .Where(c =>
                    c.CourseLevel < currentLevel &&
                    c.CourseSemester.ToLower() == currentSemester &&
                    !passedCourses.Contains(c.CourseId) &&
                    !enrolledThisYear.Contains(c.CourseId) && // 👈 المهم
                    (string.IsNullOrEmpty(c.PreReqCourseId) ||
                     passedCourses.Contains(c.PreReqCourseId))
                )
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseName,
                    c.CourseLevel,
                    c.CourseSemester,
                    c.CreditHours,
                    c.DepartmentName,
                    c.CourseType,
                    PreRequisite = c.PreReqCourseId,
                    HasPrerequisite = !string.IsNullOrEmpty(c.PreReqCourseId),
                    Source = "Lower Level (Not Passed)"
                });

            var result = currentLevelCourses
                .Concat(higherLevelCourses)
                .Concat(lowerLevelCourses)
                .OrderBy(c => c.CourseLevel)
                .ThenBy(c => c.CourseName)
                .ToList();

            return Ok(new
            {
                StudentName = student.FullName,
                CurrentLevel = currentLevel,
                CurrentSemester = currentSemester,
                GPA = student.Gpa,
                MaxCoursesAllowed = maxCourses,
                TotalAvailableCourses = result.Count,
                AvailableCourses = result
            });
        }

        // ✅ 1. جيب الكورسات المختارة
        [HttpGet("selected-courses")]
        public async Task<IActionResult> GetSelectedCourses()
        {
            var studentSSN = User.FindFirstValue("SSN");
            if (studentSSN == null) return Unauthorized("Invalid token");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == studentSSN);
            if (student == null) return NotFound("Student not found");

            int currentLevel = student.LevelId ?? 1;
            int maxCourses;
            if (currentLevel == 1 || student.Gpa == 0 || student.Gpa == null)
                maxCourses = 6;
            else if (student.Gpa >= 2)
                maxCourses = 6;
            else
                maxCourses = 4;

            var selectedCourses = await _context.SelectedCourses
                .Where(s => s.StudentSsn == studentSSN)
                .Join(_context.Courses,
                    s => s.CourseId,
                    c => c.CourseId,
                    (s, c) => new
                    {
                        c.CourseId,
                        c.CourseName,
                        c.CreditHours,
                        c.CourseLevel,
                        c.CourseSemester,
                        c.DepartmentName,
                        c.CourseType,
                        s.SelectedAt
                    })
                .ToListAsync();

            return Ok(new
            {
                StudentName = student.FullName,
                MaxCoursesAllowed = maxCourses,
                SelectedCount = selectedCourses.Count,
                RemainingSlots = maxCourses - selectedCourses.Count,
                SelectedCourses = selectedCourses
            });
        }

        [HttpPost("add-to-selected")]
        public async Task<IActionResult> AddToSelected([FromBody] AddCoursesRequest request)
        {
            var studentSSN = User.FindFirstValue("SSN");
            if (studentSSN == null) return Unauthorized("Invalid token");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == studentSSN);
            if (student == null) return NotFound("Student not found");

            int currentLevel = student.LevelId ?? 1;
            int maxCourses;

            if (currentLevel == 1 || student.Gpa == 0 || student.Gpa == null)
                maxCourses = 6;
            else if (student.Gpa >= 2)
                maxCourses = 6;
            else
                maxCourses = 4;

            var currentSelectedCount = await _context.SelectedCourses
                .CountAsync(s => s.StudentSsn == studentSSN);

            var remainingSlots = maxCourses - currentSelectedCount;

            if (request.CourseIds == null || !request.CourseIds.Any())
                return BadRequest("CourseIds list is empty");

            if (request.CourseIds.Count > remainingSlots)
                return BadRequest($"You can only add {remainingSlots} more courses");

            var passedCourses = await _context.Enrollments
                .Where(e => e.StudentSsn == studentSSN && e.Passed == true)
                .Select(e => e.CourseId)
                .ToListAsync();

            var addedCourses = new List<string>();
            var errors = new List<string>();

            foreach (var courseId in request.CourseIds)
            {
                var course = await _context.Courses.FindAsync(courseId);
                if (course == null)
                {
                    errors.Add($"{courseId}: Not found");
                    continue;
                }

                var alreadySelected = await _context.SelectedCourses
                    .AnyAsync(s => s.StudentSsn == studentSSN && s.CourseId == courseId);
                if (alreadySelected)
                {
                    errors.Add($"{courseId}: Already selected");
                    continue;
                }

                var alreadyPassed = await _context.Enrollments
                    .AnyAsync(e => e.StudentSsn == studentSSN && e.CourseId == courseId && e.Passed == true);
                if (alreadyPassed)
                {
                    errors.Add($"{courseId}: Already passed");
                    continue;
                }

                var alreadyEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.StudentSsn == studentSSN && e.CourseId == courseId && e.Passed == null);
                if (alreadyEnrolled)
                {
                    errors.Add($"{courseId}: Already enrolled");
                    continue;
                }

                if (!string.IsNullOrEmpty(course.PreReqCourseId) &&
                    !passedCourses.Contains(course.PreReqCourseId))
                {
                    errors.Add($"{courseId}: Missing prerequisite {course.PreReqCourseId}");
                    continue;
                }

                _context.SelectedCourses.Add(new SelectedCourse
                {
                    StudentSsn = studentSSN,
                    CourseId = courseId,
                    SelectedAt = DateTime.Now
                });

                addedCourses.Add(courseId);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                AddedCourses = addedCourses,
                Errors = errors,
                TotalAdded = addedCourses.Count,
                RemainingSlots = maxCourses - (currentSelectedCount + addedCourses.Count),
                MaxAllowed = maxCourses
            });
        }

        // ✅ 3. شيل كورس من الـ Selected Courses
        [HttpDelete("remove-from-selected/{courseId}")]
        public async Task<IActionResult> RemoveFromSelected(string courseId)
        {
            var studentSSN = User.FindFirstValue("SSN");
            if (studentSSN == null) return Unauthorized("Invalid token");

            var selected = await _context.SelectedCourses
                .FirstOrDefaultAsync(s => s.StudentSsn == studentSSN && s.CourseId == courseId);

            if (selected == null)
                return NotFound("Course not found in your selected list");

            _context.SelectedCourses.Remove(selected);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Course removed from selected list successfully" });
        }


        [HttpPost("confirm-enrollment")]
        public async Task<IActionResult> ConfirmEnrollment()
        {
            var studentSSN = User.FindFirstValue("SSN");
            if (studentSSN == null) return Unauthorized("Invalid token");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == studentSSN);
            if (student == null) return NotFound("Student not found");

            var selectedCourses = await _context.SelectedCourses
                .Where(s => s.StudentSsn == studentSSN)
                .ToListAsync();

            if (!selectedCourses.Any())
                return BadRequest("No courses in your selected list to confirm");

            int currentLevel = student.LevelId ?? 1;
            int maxCourses = (currentLevel == 1 || student.Gpa == 0 || student.Gpa == null || student.Gpa >= 2) ? 6 : 4;

            if (selectedCourses.Count > maxCourses)
                return BadRequest(new
                {
                    Message = $"You can only enroll in {maxCourses} courses",
                    MaxAllowed = maxCourses,
                    YouSelected = selectedCourses.Count
                });

            var enrolledList = new List<string>();
            var errors = new List<string>();
            var successfullySelected = new List<SelectedCourse>(); // 👈 المهم

            foreach (var selected in selectedCourses)
            {
                var alreadyEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.StudentSsn == studentSSN &&
                                   e.CourseId == selected.CourseId &&
                                   e.Passed == null);

                if (alreadyEnrolled)
                {
                    errors.Add($"{selected.CourseId} - Already enrolled");
                    continue;
                }

                var enrollment = new Enrollment
                {
                    CourseId = selected.CourseId,
                    StudentSsn = studentSSN,
                    EnrollmentDate = DateOnly.FromDateTime(DateTime.Now),
                    Passed = null
                };

                _context.Enrollments.Add(enrollment);

                enrolledList.Add(selected.CourseId);
                successfullySelected.Add(selected); // 👈 بنجمع الناجح بس
            }

            // 👇 نمسح الناجح بس
            //_context.SelectedCourses.RemoveRange(successfullySelected);

            //await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Enrollment confirmed successfully",
                StudentName = student.FullName,
                EnrolledCourses = enrolledList,
                Errors = errors
            });
        }
        [Authorize(Roles = "Student")]
        [HttpGet("my-enrolled-courses")]
        public async Task<IActionResult> GetMyEnrolledCourses()
        {
            // ✅ جيبي الـ SSN من الـ Token
            var ssn = User.FindFirst("SSN")?.Value;

            if (string.IsNullOrEmpty(ssn))
                return Unauthorized("SSN not found in token");

            // ✅ جيبي كل الكورسات المسجلة للطالب
            var enrolledCourses = await _context.Enrollments
                .Where(e => e.StudentSsn == ssn)
                .Include(e => e.Course)
                .Select(e => new
                {
                    CourseId = e.Course.CourseId,
                    CourseName = e.Course.CourseName,
                    CourseLevel = e.Course.CourseLevel,
                    CourseSemester = e.Course.CourseSemester,
                    CreditHours = e.Course.CreditHours,
                    EnrollmentDate = e.EnrollmentDate,
                    Passed = e.Passed
                })
                .OrderBy(c => c.CourseLevel)
                .ThenBy(c => c.CourseName)
                .ToListAsync();

            // ✅ جيبي إحصائيات
            var totalCourses = enrolledCourses.Count;
            var passedCourses = enrolledCourses.Count(c => c.Passed == true);
            var failedCourses = enrolledCourses.Count(c => c.Passed == false);

            return Ok(new
            {
                StudentSSN = ssn,
                TotalCourses = totalCourses,
                PassedCourses = passedCourses,
                FailedCourses = failedCourses,
                Courses = enrolledCourses
            });
        }
    }
}
