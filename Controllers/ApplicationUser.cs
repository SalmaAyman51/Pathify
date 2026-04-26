using Microsoft.AspNetCore.Identity;

namespace Pathify.Controllers
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsApproved { get; set; } = false;


        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string SSN { get; set; }
        public string Major { get; set; }
        public int EnrollmentYear { get; set; }
        public double GPA { get; set; }
        public string AcademicLevel { get; set; }
        public DateTime BirthDate { get; set; }
    }
}