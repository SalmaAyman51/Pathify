using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class Adminstration
{
    public string AdminSsn { get; set; } = null!;

    public string Fname { get; set; } = null!;

    public string Lname { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string ManagerSsn { get; set; } = null!;

    public virtual ICollection<AdminPhone> AdminPhones { get; set; } = new List<AdminPhone>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Adminstration> InverseManagerSsnNavigation { get; set; } = new List<Adminstration>();

    public virtual Adminstration ManagerSsnNavigation { get; set; } = null!;
}
