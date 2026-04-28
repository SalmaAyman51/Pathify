namespace Pathify.DTOs
{
    public class UpdateStudentDto
    {
        public string? Fname { get; set; }
        public string? Lname { get; set; }
        public string? Email { get; set; }
        public decimal? Gpa { get; set; }
        public string? AcademicLevel { get; set; }
        public int? EnrollmentYear { get; set; }
        public int? StudentId { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Gender { get; set; }
        public int? TeamId { get; set; }
        public int? ProjectId { get; set; }
        public int? LevelId { get; set; }
    }
}
