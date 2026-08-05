namespace Pathify.DTOs
{
    public class RegisterTeamDto
    {
        public List<TeamMemberDto> Members { get; set; }
    }

    public class TeamMemberDto
    {
        public string SSN { get; set; } = null;
        public string FullName { get; set; } = null!;
    }
}
