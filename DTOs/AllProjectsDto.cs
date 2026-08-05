namespace Pathify.DTOs
{
    public class AllProjectsDto
    {
        public int ProjectId { get; set; }
        public int TeamId { get; set; }
        public string ProjectName { get; set; } = null!;
        public string ProjectDescription { get; set; } = null!;
        public string SupervisionType { get; set; } = null!;
        public string InternalProfessorSsn { get; set; } = null!;
        public string InternalProfessorName { get; set; } = null!;
        public string ExternalProfessorSsn { get; set; } = null!;
        public string ExternalProfessorName { get; set; } = null!;
        public List<StudentInProjectDto> Students { get; set; } = new();
    }
}
