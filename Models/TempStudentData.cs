namespace Pathify.Models
{
    public class TempStudentData
    {

        
            //public int Id { get; set; }
            public string SSN { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int? StudentId { get; set; }
            public string Email { get; set; }
            public DateTime BirthDate { get; set; }
            public string Gender { get; set; }
            public int EnrollmentYear { get; set; }
            public double GPA { get; set; }
            public string AcademicLevel { get; set; }
            public int? LevelId { get; set; }
            public int? ProjectId { get; set; }
            public int? TeamId { get; set; }
            public string CurrentSemester { get; set; }
            public string PhoneNumber { get; set; }

            // ✅ الـ FK الجديد
            public string? UserId { get; set; }          // مفتاح أجنبي (string لأن IdentityUser Id بيكون string)
            public ApplicationUser? User { get; set; }    // Navigation Property
        
    }
}
