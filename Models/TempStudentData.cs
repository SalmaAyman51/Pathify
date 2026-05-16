namespace Pathify.Models
{
    public class TempStudentData
    {
        
            public string? SSN { get; set; }
            public string ?FirstName { get; set; }
            public string? LastName { get; set; }
            public int ?StudentId { get; set; }
            public string ?Email { get; set; }
            public DateTime BirthDate { get; set; }
            public string? Gender { get; set; }
            public int EnrollmentYear { get; set; }
            public double GPA { get; set; }
            public string ?AcademicLevel { get; set; }
        public string? CurrentSemester { get; set; } = "first semester";
        public int? LevelId { get; set; }

        public int? TeamId { get; set; }

        public int? ProjectId { get; set; }
        public string? PhoneNumber { get; set; }

    }
}
