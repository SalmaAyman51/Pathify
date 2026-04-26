using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class Course
{
    public string CourseId { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string CourseSemester { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public string AdminSsn { get; set; } = null!;

    public int CourseLevel { get; set; }

    public string? PreReqCourseId { get; set; }

    public int CreditHours { get; set; }

    public virtual Adminstration AdminSsnNavigation { get; set; } = null!;

    public virtual Level CourseLevelNavigation { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Course> InversePreReqCourse { get; set; } = new List<Course>();

    public virtual Course? PreReqCourse { get; set; }

    public virtual ICollection<Student> StudentSsns { get; set; } = new List<Student>();
}
