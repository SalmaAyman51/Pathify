using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public string CourseSemester { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public string AdminSsn { get; set; } = null!;

    public int CourseLevel { get; set; }

    public int? PreReqCourseId { get; set; }

    public virtual Adminstration AdminSsnNavigation { get; set; } = null!;

    public virtual Level CourseLevelNavigation { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Course> InversePreReqCourse { get; set; } = new List<Course>();

    public virtual Course? PreReqCourse { get; set; }
}
