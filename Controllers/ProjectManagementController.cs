using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pathify.DTOs;
using Pathify.Models;

namespace Pathify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ProjectManagementController : ControllerBase
    {
        private readonly PathifyContext _context;

        public ProjectManagementController(PathifyContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        private readonly UserManager<ApplicationUser> _userManager;
        private object _roleManager;

        [HttpPost("register-team")]
        public async Task<ActionResult> RegisterTeam([FromBody] RegisterTeamDto model)
        {
            var leaderSsn = User.FindFirst("SSN")?.Value;
            if (leaderSsn == null) return Unauthorized("Invalid token");

            var leader = await _context.Students.FindAsync(leaderSsn);
            if (leader == null) return NotFound("Student not found");

            if (leader.LevelId != 4)
                return BadRequest("Only level 4 students can register a team");

            if (leader.CurrentSemester != "first semester")
                return BadRequest("Team registration is only allowed in the first semester");

            // ✅ تأكد إن الليدر مش في تيم تاني
            if (leader.TeamId != null)
                return BadRequest("Leader is already in another team");

            // ✅ تأكد إن الليدر passed في 31 مادة
            var leaderPassedCourses = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentSsn == leaderSsn &&
                            e.Passed == true &&
                            e.Course.CourseLevel < leader.LevelId)
                .CountAsync();

            if (leaderPassedCourses < 31)
                return BadRequest($"Leader has not passed 31 courses from previous levels (passed: {leaderPassedCourses})");

            // ✅ جيب الـ Min و Max
            var limit = await _context.TeamLimits.FirstOrDefaultAsync();
            if (limit == null)
                return BadRequest("Team limits have not been set yet, please contact admin");

            var teamMaxMembers = limit.MaxMembers;
            var teamMinMembers = limit.MinMembers;
            var totalMembers = model.Members.Count + 1;

            if (totalMembers < teamMinMembers)
                return BadRequest($"Team must have at least {teamMinMembers} members including leader");

            if (totalMembers > teamMaxMembers)
                return BadRequest($"Team members cannot exceed {teamMaxMembers} including leader");

            var validatedStudents = new List<Student> { leader };

            foreach (var member in model.Members)
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.StudentSsn == member.SSN &&
                                             s.Fname == member.FirstName &&
                                             s.Lname == member.LastName);

                if (student == null)
                    return BadRequest($"Student {member.FirstName} {member.LastName} not found");

                if (student.LevelId != 4)
                    return BadRequest($"Student {member.FirstName} {member.LastName} must be in level 4");

                if (student.CurrentSemester != "first semester")
                    return BadRequest($"Student {member.FirstName} {member.LastName} must be in the first semester");

                // ✅ تأكد إنه مش في تيم تاني
                if (student.TeamId != null)
                    return BadRequest($"Student {member.FirstName} {member.LastName} is already in another team");

                // ✅ تأكد إنه passed في 31 مادة
                var memberPassedCourses = await _context.Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.StudentSsn == member.SSN &&
                                e.Passed == true &&
                                e.Course.CourseLevel < student.LevelId)
                    .CountAsync();

                if (memberPassedCourses < 31)
                    return BadRequest($"Student {member.FirstName} {member.LastName} has not passed 31 courses from previous levels (passed: {memberPassedCourses})");

                validatedStudents.Add(student);
            }

            // ✅ اعمل التيم في جدول Teams
            var team = new Team
            {
                LeaderSsn = leaderSsn,
                CreatedAt = DateTime.Now,
                IsApproved = true
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // ✅ حدث الـ TeamId في جدول Students لكل الأعضاء والليدر
            foreach (var student in validatedStudents)
            {
                student.TeamId = team.TeamId;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Team registered successfully", teamId = team.TeamId });
        }
    }
}
