namespace Pathify.DTOs
{
    public class RegisterTeamDto
    {
        public List<TeamMemberDto> Members { get; set; }
    }

    public class TeamMemberDto
    {
        public string SSN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
