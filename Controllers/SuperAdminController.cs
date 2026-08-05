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

    public class SuperAdminController : ControllerBase
    {
        private readonly PathifyContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SuperAdminController(PathifyContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "superadmin")]
        [HttpPost("assign-professors")]
        public async Task<IActionResult> AssignProfessors([FromBody] AssignProfessorsDto dto)
        {
            var team = await _context.Teams.FindAsync(dto.TeamId);
            if (team == null)
                return NotFound("Team not found");

            var internalProf = await _context.InternalProfessors
                .FirstOrDefaultAsync(p => p.InternalProfessorSsn == dto.InternalProfessorSsn);
            if (internalProf == null)
                return BadRequest("Internal professor not found");

            var externalProf = await _context.ExternalProfessors
                .FirstOrDefaultAsync(p => p.ExternalProfessorSsn == dto.ExternalProfessorSsn);
            if (externalProf == null)
                return BadRequest("External professor not found");

            team.InternalProfessorSsn = dto.InternalProfessorSsn;
            team.ExternalProfessorSsn = dto.ExternalProfessorSsn;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Professors assigned successfully", teamId = team.TeamId });
        }



        //[Authorize(Roles = "superadmin")]
        //[HttpGet("teams-without-professors")]
        //public async Task<IActionResult> GetTeamsWithoutProfessors()
        //{
        //    var teams = await _context.Teams
        //        .Where(t => t.InternalProfessorSsn == null || t.ExternalProfessorSsn == null)
        //        .Select(t => new
        //        {
        //            t.TeamId,
        //            t.LeaderSsn,
        //            t.CreatedAt
        //        })
        //        .ToListAsync();

        //    return Ok(teams);
        //}
        [Authorize(Roles = "superadmin")]
        [HttpGet("teams-without-professors")]
        public async Task<IActionResult> GetTeamsWithoutProfessors()
        {
            var teams = await _context.Teams
                .Where(t => t.InternalProfessorSsn == null || t.ExternalProfessorSsn == null)
                .Select(t => new
                {
                    t.TeamId,
                    t.LeaderSsn,
                    LeaderName = _context.Students
                        .Where(s => s.StudentSsn == t.LeaderSsn)
                        .Select(s => s.FullName)
                        .FirstOrDefault(),
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(teams);
        }


        [Authorize(Roles = "superadmin")]
        [HttpGet("external-professors")]
        public async Task<IActionResult> GetExternalProfessors()
        {
            var profs = await _context.ExternalProfessors
                .Select(p => new { p.ExternalProfessorSsn, p.ExternalProfessorName, p.DeptName, p.Email })
                .ToListAsync();
            return Ok(profs);
        }

        [Authorize(Roles = "superadmin")]
        [HttpGet("internal-professors")]
        public async Task<IActionResult> GetInternalProfessors()
        {
            var profs = await _context.InternalProfessors
                .Select(p => new { p.InternalProfessorSsn, p.InternalProfessorName })
                .ToListAsync();
            return Ok(profs);
        }


        [Authorize(Roles = "superadmin")]
        [HttpPost("final-approve-proposal/{proposalId}")]
        public async Task<IActionResult> FinalApproveProposal(int proposalId)
        {
            var proposal = await _context.ProjectProposals
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

            if (proposal == null) return NotFound("Proposal not found");

            if (proposal.Status != "PendingSuperAdmin")
                return BadRequest("This proposal is not ready for final approval");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var project = new Project
                {
                    TeamId = proposal.TeamId,
                    ProjectName = proposal.ProjectName,
                    ProjectDescription = proposal.ProjectDescription,
                    InternalProfessorSsn = proposal.Team.InternalProfessorSsn!,
                    ExternalProfessorSsn = proposal.Team.ExternalProfessorSsn!,
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync(); // يتولد project.ProjectId الحقيقي هنا قبل ربطه بالطلبة

                var teamStudents = await _context.Students
                    .Where(s => s.TeamId == proposal.TeamId)
                    .ToListAsync();

                foreach (var s in teamStudents)
                    s.ProjectId = project.ProjectId;

                proposal.Status = "Approved";
                proposal.ReviewedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Project approved and created", projectId = project.ProjectId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var detail = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Final approval failed: {detail}");
            }
        }




        [Authorize(Roles = "superadmin")]
        [HttpPost("final-reject-proposal/{proposalId}")]
        public async Task<IActionResult> FinalRejectProposal(int proposalId, [FromBody] ReviewDto dto)
        {
            var proposal = await _context.ProjectProposals.FindAsync(proposalId);
            if (proposal == null) return NotFound("Proposal not found");

            if (proposal.Status != "PendingSuperAdmin")
                return BadRequest("This proposal is not ready for super admin review");

            proposal.Status = "Rejected";
            proposal.RejectedBy = "SuperAdmin";   // ✅ السطر الجديد
            proposal.RejectionReason = dto.RejectionReason;
            proposal.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Proposal rejected" });
        }
        [Authorize(Roles = "superadmin")]
        [HttpGet("pending-proposals")]
        public async Task<IActionResult> GetPendingProposals()
        {
            var proposals = await _context.ProjectProposals
                .Include(p => p.Team)
                    .ThenInclude(t => t.Leader)
                .Where(p => p.Status == "PendingSuperAdmin")
                .ToListAsync();

            var result = new List<object>();

            foreach (var p in proposals)
            {
                var members = await _context.Students
                    .Where(s => s.TeamId == p.TeamId && s.StudentSsn != p.Team.LeaderSsn)
                    .Select(s => s.FullName)
                    .ToListAsync();

                result.Add(new
                {
                    proposalId = p.ProposalId,
                    title = p.ProjectName,
                    description = p.ProjectDescription,
                    date = p.CreatedAt.ToString("MMM dd, yyyy"),
                    leader = p.Team.Leader.FullName,
                    members = members
                });
            }

            return Ok(result);
        }
        [Authorize(Roles = "superadmin")]
        [HttpGet("approved-projects")]
        public async Task<IActionResult> GetApprovedProjects()
        {
            var projects = await _context.Projects.ToListAsync();

            var result = new List<object>();

            foreach (var p in projects)
            {
                var team = await _context.Teams
                    .Include(t => t.Leader)
                    .FirstOrDefaultAsync(t => t.TeamId == p.TeamId);

                var members = await _context.Students
                    .Where(s => s.TeamId == p.TeamId && s.StudentSsn != team!.LeaderSsn)
                    .Select(s => s.Fname)
                    .ToListAsync();

                result.Add(new
                {
                    id = p.ProjectId,
                    title = p.ProjectName,
                    description = p.ProjectDescription,
                    date = team?.CreatedAt?.ToString("MMM dd, yyyy") ?? "",
                    leader = team?.Leader?.FullName ?? "",
                    members = members
                });
            }

            return Ok(result);
        }

        [Authorize(Roles = "superadmin")]
        [HttpGet("total-projects-count")]
        public async Task<IActionResult> GetTotalProjectsCount()
        {
            var count = await _context.Projects.CountAsync();
            return Ok(new { totalProjects = count });
        }

        [Authorize(Roles = "superadmin")]
        [HttpGet("pending-projects-count")]
        public async Task<IActionResult> GetPendingProjectsCount()
        {
            var count = await _context.ProjectProposals
                .CountAsync(p => p.Status == "PendingSuperAdmin");
            return Ok(new { pendingProjects = count });
        }

        [Authorize(Roles = "superadmin")]
        [HttpGet("pending-teams-count")]
        public async Task<IActionResult> GetPendingTeamsCount()
        {
            var count = await _context.Teams
                .CountAsync(t => t.InternalProfessorSsn == null || t.ExternalProfessorSsn == null);
            return Ok(new { pendingTeams = count });
        }
        [HttpPost("team-limit")]
        [Authorize(Roles = "superadmin")]
        public async Task<IActionResult> SetTeamLimit([FromBody] TeamLimitDto dto)
        {
            if (dto.MinMembers <= 0 || dto.MaxMembers <= 0 || dto.MinMembers > dto.MaxMembers)
                return BadRequest("Invalid team size values.");

            var existing = await _context.TeamLimits
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.MinMembers = dto.MinMembers;
                existing.MaxMembers = dto.MaxMembers;
            }
            else
            {
                _context.TeamLimits.Add(new TeamLimit
                {
                    MinMembers = dto.MinMembers,
                    MaxMembers = dto.MaxMembers
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Team limit updated successfully." });
        }

    }
}
