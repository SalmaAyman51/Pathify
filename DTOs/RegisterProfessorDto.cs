using System.ComponentModel.DataAnnotations;

namespace Pathify.DTOs
{
    public class RegisterProfessorDto
    {
        public string SSN { get; set; } = null!;
        public string ProfessorType { get; set; } = null!; // "Internal" or "External"
        public string Password { get; set; } = null!;
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
