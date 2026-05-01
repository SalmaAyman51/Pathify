using System.ComponentModel.DataAnnotations;

namespace Pathify.DTOs
{
    public class CourseDTO
    {
        

public class CourseDto
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "CourseId must be letters and numbers only")]
        public string CourseId { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "CourseName must be letters only")]
        public string CourseName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "CourseSemester must be letters only")]
        public string CourseSemester { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "DepartmentName must be letters only")]
        public string DepartmentName { get; set; }

        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "AdminSsn must be numbers only")]
        public string AdminSsn { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "CourseLevel must be a positive number")]
        public int CourseLevel { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "PreReqCourseId must be letters and numbers only")]
        public string? PreReqCourseId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "CreditHours must be a positive number")]
        public int CreditHours { get; set; }

            [Required]
            [RegularExpression(@"^(Mandatory|Elective)$", ErrorMessage = "CourseType must be 'Mandatory' or 'Elective'")]
            public string CourseType { get; set; }
        }
}
}

