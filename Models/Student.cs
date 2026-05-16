using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class Student
{
    public string StudentSsn { get; set; } = null!;

    public int? StudentId { get; set; }

    public string Fname { get; set; } = null!;

    public string Lname { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public decimal? Gpa { get; set; }

    public DateOnly BirthDate { get; set; }

    public string Gender { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int? EnrollmentYear { get; set; }

    public string? AcademicLevel { get; set; }

    public int? LevelId { get; set; }

    public int? TeamId { get; set; }

    public int? ProjectId { get; set; }

    public bool IsApproved { get; set; }
    public string? CurrentSemester { get; set; } = "first semester";
    public string? PhoneNumber { get; set; }
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Project? Project { get; set; }

    //public virtual ICollection<StudentPhone> StudentPhones { get; set; } = new List<StudentPhone>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
