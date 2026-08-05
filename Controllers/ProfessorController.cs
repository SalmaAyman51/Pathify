using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pathify.DTOs;
using Pathify.Models;
using System.Security.Claims;

namespace Pathify.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessorController : ControllerBase
    {
        private readonly PathifyContext _context;

        public ProfessorController(PathifyContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Get()
        {
            return Ok("You have accessed the Professor controller.");
        }

        [Authorize(Roles = "InternalProfessor")]
        [HttpPost("review-proposal-internal/{proposalId}")]
        public async Task<IActionResult> ReviewProposalInternal(int proposalId, [FromBody] ReviewDto dto)
        {
            var profSsn = User.FindFirst("SSN")?.Value;
            if (profSsn == null) return Unauthorized("Invalid token");

            var proposal = await _context.ProjectProposals
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

            if (proposal == null) return NotFound("Proposal not found");

            if (proposal.Team.InternalProfessorSsn != profSsn)
                return StatusCode(403, "You are not the internal professor for this team");

            if (proposal.Status != "PendingProfessors")
                return BadRequest("This proposal is not pending professor review");

            if (proposal.InternalApproval != "Pending")
                return BadRequest("You have already reviewed this proposal");

            proposal.InternalApproval = dto.Approved ? "Approved" : "Rejected";
            if (!dto.Approved)
                proposal.InternalRejectionReason = dto.RejectionReason;

            if (proposal.InternalApproval != "Pending" && proposal.ExternalApproval != "Pending")
            {
                if (proposal.InternalApproval == "Rejected" || proposal.ExternalApproval == "Rejected")
                {
                    proposal.Status = "Rejected";
                    proposal.RejectedBy = "Supervisor";
                }
                else
                {
                    proposal.Status = "PendingSuperAdmin";
                }
                proposal.ReviewedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Review submitted", status = proposal.Status });
        }


        [Authorize(Roles = "ExternalProfessor")]
        [HttpPost("review-proposal-external/{proposalId}")]
        public async Task<IActionResult> ReviewProposalExternal(int proposalId, [FromBody] ReviewDto dto)
        {
            var profSsn = User.FindFirst("SSN")?.Value;
            if (profSsn == null) return Unauthorized("Invalid token");

            var proposal = await _context.ProjectProposals
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

            if (proposal == null) return NotFound("Proposal not found");

            if (proposal.Team.ExternalProfessorSsn != profSsn)
                return StatusCode(403, "You are not the external professor for this team");

            if (proposal.Status != "PendingProfessors")
                return BadRequest("This proposal is not pending professor review");

            if (proposal.ExternalApproval != "Pending")
                return BadRequest("You have already reviewed this proposal");

            proposal.ExternalApproval = dto.Approved ? "Approved" : "Rejected";
            if (!dto.Approved)
                proposal.ExternalRejectionReason = dto.RejectionReason;

            if (proposal.InternalApproval != "Pending" && proposal.ExternalApproval != "Pending")
            {
                if (proposal.InternalApproval == "Rejected" || proposal.ExternalApproval == "Rejected")
                {
                    proposal.Status = "Rejected";
                    proposal.RejectedBy = "Supervisor";
                }
                else
                {
                    proposal.Status = "PendingSuperAdmin";
                }
                proposal.ReviewedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Review submitted", status = proposal.Status });
        }


        // في ProfessorController.cs


        // =================== INTERNAL ===================

        [Authorize(Roles = "InternalProfessor")]
[HttpGet("internal/supervised-teams")]
public async Task<IActionResult> GetInternalSupervisedTeams()
        {
            var profSsn = User.FindFirst("SSN")?.Value;
            if (profSsn == null) return Unauthorized("Invalid token");

            var supervisedTeams = await _context.Teams
     .Where(t => t.InternalProfessorSsn == profSsn)
     .Include(t => t.Leader)
     .Select(t => new
     {
         t.TeamId,
         LeaderName = t.Leader.FullName,
         t.CreatedAt,
         Members = _context.Students
        .Where(s => s.TeamId == t.TeamId)
        .Select(s => new
        {
            
            s.Fname
        }).ToList()
     })
     .ToListAsync();

            return Ok(supervisedTeams);
        }

        [Authorize(Roles = "InternalProfessor")]
        [HttpGet("internal/pending-proposals")]
        public async Task<IActionResult> GetInternalPendingProposals()
        {
            var profSsn = User.FindFirst("SSN")?.Value;
            if (profSsn == null) return Unauthorized("Invalid token");

            var pendingProposals = await _context.ProjectProposals
                .Include(p => p.Team)
                    .ThenInclude(t => t.Leader)
                .Where(p => p.Status == "PendingProfessors"
                         && p.InternalApproval == "Pending"
                         && p.Team.InternalProfessorSsn == profSsn)
                .Select(p => new
                {
                    p.ProposalId,
                    p.ProjectName,
                    p.ProjectDescription,
                    p.CreatedAt,
                    TeamId = p.Team.TeamId,
                    LeaderName = p.Team.Leader.FullName
                })
                .ToListAsync();

            return Ok(pendingProposals);
        }

        // =================== EXTERNAL ===================

        [Authorize(Roles = "ExternalProfessor")]
        [HttpGet("external/supervised-teams")]
        public async Task<IActionResult> GetExternalSupervisedTeams()
        {
            var profSsn = User.FindFirst("SSN")?.Value;
            if (profSsn == null) return Unauthorized("Invalid token");

            var supervisedTeams = await _context.Teams
     .Where(t => t.ExternalProfessorSsn == profSsn)
     .Include(t => t.Leader)
     .Select(t => new
     {
         t.TeamId,
         LeaderName = t.Leader.FullName,
         t.CreatedAt,
         Members = _context.Students
        .Where(s => s.TeamId == t.TeamId)
        .Select(s => new
        {

            s.Fname
        }).ToList()
     })
     .ToListAsync();

            return Ok(supervisedTeams);
        }

        [Authorize(Roles = "ExternalProfessor")]
        [HttpGet("external/pending-proposals")]
        public async Task<IActionResult> GetExternalPendingProposals()
        {
            var profSsn = User.FindFirst("SSN")?.Value;
            if (profSsn == null) return Unauthorized("Invalid token");

            var pendingProposals = await _context.ProjectProposals
                .Include(p => p.Team)
                    .ThenInclude(t => t.Leader)
                .Where(p => p.Status == "PendingProfessors"
                         && p.ExternalApproval == "Pending"
                         && p.Team.ExternalProfessorSsn == profSsn)
                .Select(p => new
                {
                    p.ProposalId,
                    p.ProjectName,
                    p.ProjectDescription,
                    p.CreatedAt,
                    TeamId = p.Team.TeamId,
                    LeaderName = p.Team.Leader.FullName
                })
                .ToListAsync();

            return Ok(pendingProposals);
        }
        [Authorize(Roles = "InternalProfessor")]
        [HttpGet("internal/supervised-teams-count")]
        public async Task<IActionResult> GetSupervisedTeamsCount()
        {
            var ssn = User.FindFirst("ssn")?.Value; // أو أي claim بتستخدميه لتحديد هوية الدكتور
            if (ssn == null) return Unauthorized();

            var count = await _context.Teams
                .CountAsync(t => t.InternalProfessorSsn == ssn);

            return Ok(new { supervisedTeamsCount = count });
        }

        [Authorize(Roles = "InternalProfessor")]
        [HttpGet("internal/pending-proposals-count")]
        public async Task<IActionResult> GetPendingProposalsCount()
        {
            var ssn = User.FindFirst("ssn")?.Value;
            if (ssn == null) return Unauthorized();

            var count = await _context.ProjectProposals
                .Include(p => p.Team)
                .CountAsync(p => p.Status == "PendingProfessors"
                               && p.Team.InternalProfessorSsn == ssn
                               && p.InternalApproval == "Pending");

            return Ok(new { pendingProposalsCount = count });
        }


        [Authorize(Roles = "ExternalProfessor")]
        [HttpGet("external/supervised-teams-count")]
        public async Task<IActionResult> GetSupervisedTeamsCountExternal()
        {
            var ssn = User.FindFirst("ssn")?.Value;
            if (ssn == null) return Unauthorized();

            var count = await _context.Teams
                .CountAsync(t => t.ExternalProfessorSsn == ssn);

            return Ok(new { supervisedTeamsCount = count });
        }

        [Authorize(Roles = "ExternalProfessor")]
        [HttpGet("external/pending-proposals-count")]
        public async Task<IActionResult> GetPendingProposalsCountExternal()
        {
            var ssn = User.FindFirst("SSN")?.Value;
            if (ssn == null) return Unauthorized();

            var count = await _context.ProjectProposals
                .Include(p => p.Team)
                .CountAsync(p => p.Status == "PendingProfessors"
                               && p.Team.ExternalProfessorSsn == ssn
                               && p.ExternalApproval == "Pending");

            return Ok(new { pendingProposalsCount = count });
        }

        [Authorize(Roles = "InternalProfessor,ExternalProfessor")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfessorProfile()
        {
            var ssn = User.FindFirst("SSN")?.Value;
            if (ssn == null) return Unauthorized();

            bool isExternal = User.IsInRole("ExternalProfessor");

            if (isExternal)
            {
                var prof = await _context.ExternalProfessors
                    .Include(p => p.ExternalProfessorPhones)
                    .FirstOrDefaultAsync(p => p.ExternalProfessorSsn == ssn);

                if (prof == null) return NotFound();

                return Ok(new
                {
                    ssn = prof.ExternalProfessorSsn,
                    name = prof.ExternalProfessorName,
                    deptName = prof.DeptName,
                    email = prof.Email,
                    phone = prof.ExternalProfessorPhones.FirstOrDefault()?.PhoneNumber,
                    role = "External"
                });
            }
            else
            {
                var prof = await _context.InternalProfessors
                    .Include(p => p.InternalProfessorPhones)
                    .FirstOrDefaultAsync(p => p.InternalProfessorSsn == ssn);

                if (prof == null) return NotFound();

                return Ok(new
                {
                    ssn = prof.InternalProfessorSsn,
                    name = prof.InternalProfessorName,
                    deptName = prof.DeptName,
                    email = prof.Email,
                    phone = prof.InternalProfessorPhones.FirstOrDefault()?.PhoneNumber,
                    role = "Internal"
                });
            }
        }



        [Authorize(Roles = "InternalProfessor,ExternalProfessor")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfessorProfile([FromBody] UpdateProfessorProfileDto dto)
        {
            var ssn = User.FindFirst("SSN")?.Value;
            if (ssn == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");

            if (!string.IsNullOrWhiteSpace(dto.Email) && !IsValidEmail(dto.Email))
                return BadRequest("Invalid email format.");

            bool isExternal = User.IsInRole("ExternalProfessor");

            if (isExternal)
            {
                var prof = await _context.ExternalProfessors
                    .Include(p => p.ExternalProfessorPhones)
                    .FirstOrDefaultAsync(p => p.ExternalProfessorSsn == ssn);

                if (prof == null) return NotFound();

                prof.ExternalProfessorName = dto.Name.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Email))
                    prof.Email = dto.Email.Trim();

                var phoneRow = prof.ExternalProfessorPhones.FirstOrDefault();
                if (phoneRow != null)
                {
                    phoneRow.PhoneNumber = dto.Phone?.Trim() ?? phoneRow.PhoneNumber;
                }
                else if (!string.IsNullOrWhiteSpace(dto.Phone))
                {
                    _context.Add(new ExternalProfessorPhone
                    {
                        ExternalProfessorSsn = prof.ExternalProfessorSsn,
                        PhoneNumber = dto.Phone.Trim()
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    name = prof.ExternalProfessorName,
                    phone = dto.Phone,
                    email = prof.Email
                });
            }
            else
            {
                var prof = await _context.InternalProfessors
                    .Include(p => p.InternalProfessorPhones)
                    .FirstOrDefaultAsync(p => p.InternalProfessorSsn == ssn);

                if (prof == null) return NotFound();

                prof.InternalProfessorName = dto.Name.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Email))
                    prof.Email = dto.Email.Trim();

                var phoneRow = prof.InternalProfessorPhones.FirstOrDefault();
                if (phoneRow != null)
                {
                    phoneRow.PhoneNumber = dto.Phone?.Trim() ?? phoneRow.PhoneNumber;
                }
                else if (!string.IsNullOrWhiteSpace(dto.Phone))
                {
                    _context.Add(new InternalProfessorPhone
                    {
                        InternalProfessorSsn = prof.InternalProfessorSsn,
                        PhoneNumber = dto.Phone.Trim()
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    name = prof.InternalProfessorName,
                    phone = dto.Phone,
                    email = prof.Email
                });
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        [Authorize(Roles = "InternalProfessor,ExternalProfessor")]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentProfessor()
        {
            var ssn = User.FindFirst("SSN")?.Value;
            if (ssn == null) return Unauthorized();

            bool isExternal = User.IsInRole("ExternalProfessor");

            if (isExternal)
            {
                var prof = await _context.ExternalProfessors
                    .FirstOrDefaultAsync(p => p.ExternalProfessorSsn == ssn);

                if (prof == null) return NotFound();

                return Ok(new
                {
                    name = prof.ExternalProfessorName,
                    role = "External"
                });
            }
            else
            {
                var prof = await _context.InternalProfessors
                    .FirstOrDefaultAsync(p => p.InternalProfessorSsn == ssn);

                if (prof == null) return NotFound();

                return Ok(new
                {
                    name = prof.InternalProfessorName,
                    role = "Internal"
                });
            }
        }
        [HttpGet("all-projects")]
        [Authorize(Roles = "InternalProfessor,ExternalProfessor")]
        public async Task<IActionResult> GetAllProjects()
        {
            var ssn = User.FindFirstValue("SSN")
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(ssn))
                return Unauthorized("SSN claim not found in token");

            // 1) هات المشاريع اللي الدكتور ده مشرف عليها
            var projects = await _context.Projects
                .Include(p => p.InternalProfessorSsnNavigation)
                .Include(p => p.ExternalProfessorSsnNavigation)
                .Where(p => p.InternalProfessorSsn == ssn || p.ExternalProfessorSsn == ssn)
                .ToListAsync();

            if (!projects.Any())
                return Ok(new List<AllProjectsDto>());

            var teamIds = projects.Select(p => p.TeamId).ToList();

            // 2) هات كل الطلاب اللي TeamId بتاعهم من ضمن الفرق دي
            //    (الطلاب في نفس الفريق بياخدوا نفس الـ TeamId في جدول Students)
            var students = await _context.Students
                .Where(s => s.TeamId != null && teamIds.Contains(s.TeamId.Value))
                .ToListAsync();

            var studentsByTeam = students
                .GroupBy(s => s.TeamId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(s => new StudentInProjectDto
                {
                    StudentSsn = s.StudentSsn,
                    Fname = s.Fname,
                    Lname = s.Lname
                }).ToList());

            // 3) ابني الـ DTO النهائي
            var result = projects.Select(p => new AllProjectsDto
            {
                ProjectId = p.ProjectId,
                TeamId = p.TeamId,
                ProjectName = p.ProjectName,
                ProjectDescription = p.ProjectDescription,

                SupervisionType = p.InternalProfessorSsn == ssn ? "Internal" : "External",

                InternalProfessorSsn = p.InternalProfessorSsn,
                InternalProfessorName = p.InternalProfessorSsnNavigation?.InternalProfessorName,

                ExternalProfessorSsn = p.ExternalProfessorSsn,
                ExternalProfessorName = p.ExternalProfessorSsnNavigation?.ExternalProfessorName,

                Students = studentsByTeam.TryGetValue(p.TeamId, out var members)
                    ? members
                    : new List<StudentInProjectDto>()
            }).ToList();

            return Ok(result);
        }
    }
    }  


