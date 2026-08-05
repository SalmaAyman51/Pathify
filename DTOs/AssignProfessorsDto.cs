namespace Pathify.DTOs
{
    public class AssignProfessorsDto
    {
        public int TeamId { get; set; }
        public string InternalProfessorSsn { get; set; } = null!;
        public string ExternalProfessorSsn { get; set; } = null!;
    }
}
