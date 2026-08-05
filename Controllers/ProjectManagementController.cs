using Microsoft.AspNetCore.Authorization;
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

            if (!string.Equals(leader.CurrentSemester, "first semester", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Team registration is only allowed in the first semester");

            if (leader.TeamId != null)
                return BadRequest("Leader is already in another team");

            var leaderPassedCourses = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentSsn == leaderSsn &&
                            e.Passed == PassStatus.Passed &&
                            e.Course.CourseLevel < leader.LevelId)
                .CountAsync();

            if (leaderPassedCourses < 31)
                return BadRequest($"Leader has not passed 31 courses from previous levels (passed: {leaderPassedCourses})");

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
            var memberErrors = new List<string>();

            foreach (var member in model.Members)
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.StudentSsn == member.SSN &&
                                             s.FullName == member.FullName);

                if (student == null)
                {
                    memberErrors.Add($"{member.FullName}: Student not found (check SSN/name match)");
                    continue;
                }

                var studentIssues = new List<string>();

                if (student.LevelId != 4)
                    studentIssues.Add("not in level 4");

                if (!string.Equals(student.CurrentSemester, "first semester", StringComparison.OrdinalIgnoreCase))
                    studentIssues.Add("not in first semester");

                if (student.TeamId != null)
                    studentIssues.Add("already in another team");

                var memberPassedCourses = await _context.Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.StudentSsn == member.SSN &&
                                e.Passed == PassStatus.Passed &&
                                e.Course.CourseLevel < student.LevelId)
                    .CountAsync();

                if (memberPassedCourses < 31)
                    studentIssues.Add($"has not passed 31 courses (passed: {memberPassedCourses})");

                if (studentIssues.Any())
                {
                    memberErrors.Add($"{member.FullName}: {string.Join(", ", studentIssues)}");
                }
                else
                {
                    validatedStudents.Add(student);
                }
            }

            if (memberErrors.Any())
                return BadRequest(new { message = "Some members have issues", errors = memberErrors });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var team = new Team
                {
                    LeaderSsn = leaderSsn,
                    CreatedAt = DateTime.Now,
                };

                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                foreach (var student in validatedStudents)
                {
                    student.TeamId = team.TeamId;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { message = "Team registered successfully", teamId = team.TeamId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Team registration failed: {ex.Message}");
            }
        }


        [Authorize(Roles = "Student")]
        [HttpPost("submit-proposal")]
        public async Task<IActionResult> SubmitProposal([FromBody] SubmitProposalDto dto)
        {
            var leaderSsn = User.FindFirst("SSN")?.Value;
            if (leaderSsn == null) return Unauthorized("Invalid token");

            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.LeaderSsn == leaderSsn);

            if (team == null)
                return BadRequest("You are not a team leader");

            if (team.InternalProfessorSsn == null || team.ExternalProfessorSsn == null)
                return BadRequest("Your team doesn't have assigned professors yet, please contact admin");

            var hasPendingProposal = await _context.ProjectProposals
                .AnyAsync(p => p.TeamId == team.TeamId &&
                               (p.Status == "PendingProfessors" || p.Status == "PendingSuperAdmin"));

            if (hasPendingProposal)
                return BadRequest("Your team already has a pending proposal");

            var proposal = new ProjectProposal
            {
                TeamId = team.TeamId,
                ProjectName = dto.ProjectName,
                ProjectDescription = dto.ProjectDescription,
                Status = "PendingProfessors",
                InternalApproval = "Pending",
                ExternalApproval = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.ProjectProposals.Add(proposal);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Proposal submitted successfully", proposalId = proposal.ProposalId });
        }




        [Authorize(Roles = "Student")]
        [HttpGet("my-info")]
        public async Task<IActionResult> GetMyInfo()
        {
            var ssn = User.FindFirst("SSN")?.Value;
            if (ssn == null) return Unauthorized();

            var student = await _context.Students.FindAsync(ssn);
            if (student == null) return NotFound();

            return Ok(new
            {
                fullName = student.FullName,
                ssn = student.StudentSsn
            });
        }

        [Authorize(Roles = "Student")]
        [HttpGet("my-proposal")]
        public async Task<IActionResult> GetMyProposal()
        {
            var studentSsn = User.FindFirst("SSN")?.Value;
            if (studentSsn == null) return Unauthorized("Invalid token");

            // نجيب الـ team بتاع الطالب
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == studentSsn);

            if (student == null || student.TeamId == null)
                return NotFound("No team found");

            var proposal = await _context.ProjectProposals
                .Where(p => p.TeamId == student.TeamId)
                .Select(p => new
                {
                    p.ProjectName,
                    p.ProjectDescription,
                    p.Status,
                    p.InternalApproval,
                    p.ExternalApproval,
                    p.RejectionReason,
                    p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (proposal == null)
                return NotFound("No proposal found");

            return Ok(proposal);
        }


        [Authorize(Roles = "Student")]
        [HttpGet("my-status")]
        public async Task<IActionResult> GetMyStatus()
        {
            var studentSsn = User.FindFirst("SSN")?.Value;
            if (studentSsn == null) return Unauthorized("Invalid token");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == studentSsn);

            if (student == null) return NotFound("Student not found");

            // لا team
            if (student.TeamId == null)
                return Ok(new { step = 0, subStep = "noTeam" });

            // ✅ نجيب الـ Team عشان نتأكد من الـ professors
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.TeamId == student.TeamId);

            if (team == null)
                return Ok(new { step = 0, subStep = "noTeam" });

            bool supervisorsAssigned = !string.IsNullOrEmpty(team.InternalProfessorSsn)
                                     && !string.IsNullOrEmpty(team.ExternalProfessorSsn);

            // عنده team، نشوف الـ proposal
            var proposal = await _context.ProjectProposals
       .Where(p => p.TeamId == student.TeamId)
       .Select(p => new
       {
           p.Status,
           p.InternalApproval,
           p.ExternalApproval,
           p.RejectionReason,
           p.InternalRejectionReason,   // ✅ جديد
           p.ExternalRejectionReason,   // ✅ جديد
           p.RejectedBy
       })
       .FirstOrDefaultAsync();

            // عنده team بس لا proposal لسه
            if (proposal == null)
            {
                // ✅ لو الـ professors اتعينوا، يدخل على مرحلة تقديم الفكرة
                if (supervisorsAssigned)
                    return Ok(new { step = 0, subStep = "waitingProposal" });

                // لسه مفيش professors متعينين
                return Ok(new { step = 0, subStep = "waitingSupervisor" });
            }

            // عنده proposal
            if (proposal.Status == "PendingProfessors")
                return Ok(new { step = 1, subStep = "pendingProfessors", proposal });

            if (proposal.Status == "Rejected")
                return Ok(new { step = 1, subStep = "rejected", proposal });

            if (proposal.Status == "PendingSuperAdmin")
                return Ok(new { step = 1, subStep = "pendingSuperAdmin", proposal });

            if (proposal.Status == "Approved")
                return Ok(new { step = 3, subStep = "approved", proposal });

            if (student.ProjectId != null)
                return Ok(new { step = 3, subStep = "approved", proposal });

            return Ok(new { step = 1, subStep = "pendingSuperAdmin", proposal });
        }
        [Authorize(Roles = "Student")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetStudentProfile()
        {
            var ssn = User.FindFirst("SSN")?.Value;
            if (ssn == null) return Unauthorized();

            var student = await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.StudentSsn == ssn);

            if (student == null) return NotFound();

            string? levelName = null;
            if (student.LevelId != null)
            {
                levelName = await _context.Levels
                    .Where(l => l.LevelId == student.LevelId)
                    .Select(l => l.LevelName)
                    .FirstOrDefaultAsync();
            }

            bool isSeniorStudent = student.LevelId == 4;

            var courses = student.Enrollments.Select(e => new
            {
                code = e.CourseId,
                name = e.Course.CourseName,
                hours = e.Course.CreditHours,
                status = e.Passed == PassStatus.Passed ? "Passed" : e.Passed == PassStatus.Failed ? "Failed" : "Registered",
                semester = e.Course.CourseSemester
            }).ToList();

            object? teamInfo = null;

            if (isSeniorStudent && student.TeamId != null)
            {
                var team = await _context.Teams
                    .FirstOrDefaultAsync(t => t.TeamId == student.TeamId);

                if (team != null)
                {
                    var project = await _context.Projects
                        .FirstOrDefaultAsync(p => p.TeamId == team.TeamId);

                    var members = await _context.Students
                        .Where(s => s.TeamId == team.TeamId)
                        .Select(s => new
                        {
                            name = s.FullName,
                            isLeader = s.StudentSsn == team.LeaderSsn
                        })
                        .ToListAsync();

                    teamInfo = new
                    {
                        projectName = project != null ? project.ProjectName : null,
                        description = project != null ? project.ProjectDescription : null,
                        members
                    };
                }
            }

            return Ok(new
            {
                name = student.FullName,
                email = student.Email,
                phone = student.PhoneNumber,
                studentId = student.StudentId,
                level = levelName,
                semester = student.CurrentSemester,
                isSeniorStudent,
                team = teamInfo,
                courses
            });
        }


        [Authorize(Roles = "Student")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateStudentProfile([FromBody] UpdateStudentProfileDto dto)
        {
            var ssn = User.FindFirst("SSN")?.Value;
            if (ssn == null) return Unauthorized();

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == ssn);

            if (student == null) return NotFound();

            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");
            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email is required.");

            // FullName is updated directly here; adjust if Fname/Lname
            // need to be kept in sync separately.
            student.FullName = dto.Name.Trim();
            student.Email = dto.Email.Trim();
            student.PhoneNumber = dto.Phone?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                name = student.FullName,
                email = student.Email,
                phone = student.PhoneNumber
            });
        }

        [Authorize(Roles = "Student")]
        [HttpGet("name")]
        public async Task<IActionResult> GetStudentName()
        {
            var ssn = User.FindFirst("SSN")?.Value;
            if (ssn == null) return Unauthorized();

            var fullName = await _context.Students
                .Where(s => s.StudentSsn == ssn)
                .Select(s => s.FullName)
                .FirstOrDefaultAsync();

            if (fullName == null) return NotFound();

            return Ok(new { name = fullName });
        }

        [HttpGet("past-projects")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetPastYearsProjects()
        {
            var projects = await _context.PastYearsProjects
                .OrderByDescending(p => p.Year)
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("team-limit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetTeamLimit()
        {
            var teamLimit = await _context.TeamLimits
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            if (teamLimit == null)
                return Ok(new { minMembers = 2, maxMembers = 6 }); // fallback افتراضي لو الجدول فاضي

            return Ok(new
            {
                minMembers = teamLimit.MinMembers,
                maxMembers = teamLimit.MaxMembers
            });
        }


        [Authorize(Roles = "Student")]
        [HttpGet("my-supervisors")]
        public async Task<IActionResult> GetMySupervisors()
        {
            var studentSsn = User.FindFirst("SSN")?.Value;
            if (studentSsn == null) return Unauthorized("Invalid token");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == studentSsn);

            if (student == null) return NotFound("Student not found");
            if (student.TeamId == null) return NotFound("No team found for this student");

            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.TeamId == student.TeamId);

            if (team == null) return NotFound("Team not found");

            if (string.IsNullOrEmpty(team.InternalProfessorSsn) || string.IsNullOrEmpty(team.ExternalProfessorSsn))
                return BadRequest("Supervisors are not fully assigned yet");

            var internalProf = await _context.InternalProfessors
                .Include(p => p.InternalProfessorPhones)
                .FirstOrDefaultAsync(p => p.InternalProfessorSsn == team.InternalProfessorSsn);

            var externalProf = await _context.ExternalProfessors
                .Include(p => p.ExternalProfessorPhones)
                .FirstOrDefaultAsync(p => p.ExternalProfessorSsn == team.ExternalProfessorSsn);

            if (internalProf == null || externalProf == null)
                return NotFound("One or both supervisors not found in system");

            return Ok(new
            {
                internal_ = new
                {
                    name = internalProf.InternalProfessorName,
                    email = internalProf.Email,
                    dept = internalProf.DeptName,
                    phone = internalProf.InternalProfessorPhones.Select(p => p.PhoneNumber).FirstOrDefault()
                },
                external = new
                {
                    name = externalProf.ExternalProfessorName,
                    email = externalProf.Email,
                    dept = externalProf.DeptName,
                    phone = externalProf.ExternalProfessorPhones.Select(p => p.PhoneNumber).FirstOrDefault()
                }
            });
        }


        [Authorize(Roles = "Student")]
        [HttpPost("resubmit-proposal")]
        public async Task<IActionResult> ResubmitProposal()
        {
            var studentSsn = User.FindFirst("SSN")?.Value;
            if (studentSsn == null) return Unauthorized("Invalid token");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == studentSsn);

            if (student == null) return NotFound("Student not found");
            if (student.TeamId == null) return BadRequest("No team found");

            var proposal = await _context.ProjectProposals
                .FirstOrDefaultAsync(p => p.TeamId == student.TeamId);

            if (proposal == null)
                return BadRequest("No proposal to resubmit");

            if (proposal.Status != "Rejected")
                return BadRequest("Only rejected proposals can be resubmitted");

            // ✅ نمسح البروبوزال المرفوض عشان الطالب يبعت فكرة جديدة من الأول
            _context.ProjectProposals.Remove(proposal);
            await _context.SaveChangesAsync();

            return Ok(new { message = "You can now submit a new idea" });
        }
    }
}