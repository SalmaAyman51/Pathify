using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class Enrollment
{
    public int CourseId { get; set; }

    public string StudentSsn { get; set; } = null!;

    public DateOnly? EnrollmentDate { get; set; }

    public string? AdminSsn { get; set; }

    public bool? Passed { get; set; }

    public virtual Adminstration? AdminSsnNavigation { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Student StudentSsnNavigation { get; set; } = null!;
}
