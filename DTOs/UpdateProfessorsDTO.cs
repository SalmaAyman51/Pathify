namespace Pathify.DTOs
{
    public class UpdateProfessorsDTO
    {
        public class UpdateExternalProfessorDto
        {
            public string? ExternalProfessorName { get; set; }
            public string? DeptName { get; set; }
            public string? Email { get; set; }
            public List<ExternalProfessorPhoneDto>? ExternalProfessorPhones { get; set; }
        }

        public class ExternalProfessorPhoneDto
        {
            public string? PhoneNumber { get; set; }
        }

        public class UpdateInternalProfessorDto
        {
            public string? InternalProfessorName { get; set; }
            public string? DeptName { get; set; }
            public string? Email { get; set; }
            public List<InternalProfessorPhoneDto>? InternalProfessorPhones { get; set; }
        }

        public class InternalProfessorPhoneDto
        {
            public string? PhoneNumber { get; set; }
        }
    }
}
