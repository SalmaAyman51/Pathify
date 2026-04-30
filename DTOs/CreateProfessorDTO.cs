namespace Pathify.DTOs
{
    public class CreateProfessorDTO
    {

        public class CreateInternalProfessorDTO
        {
            public string SSN { get; set; }
            public string FullName { get; set; }
            public string DeptName { get; set; }
            public string Password { get; set; }
            public string? PhoneNumber { get; set; }
        }

        public class CreateExternalProfessorDTO
        {
            public string SSN { get; set; }
            public string FullName { get; set; }
            public string DeptName { get; set; }
            public string Password { get; set; }
            public string? PhoneNumber { get; set; }
        }

    }
}
