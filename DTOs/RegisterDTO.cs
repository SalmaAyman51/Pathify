using System.ComponentModel.DataAnnotations;

namespace Pathify.DTOs
{
    public class RegisterDTO
    {

       
    public class RegisterDto
    {
        [Required]
        public string SSN { get; set; } = null!;

        [Required]
        public int StudentId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        public DateOnly BirthDate { get; set; }

        [Required]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be Male or Female")]
        public string Gender { get; set; } = null!;

        public int? EnrollmentYear { get; set; }

        [Range(0.0, 4.0)]
        public decimal? GPA { get; set; }

        public string? AcademicLevel { get; set; }

        public int? LevelId { get; set; }

        public int? ProjectId { get; set; }

        public int? TeamId { get; set; }

        [Required]
        public string Role { get; set; } = "Student";
            public string? CurrentSemester { get; set; } = "first semester";

        }
    }
}
