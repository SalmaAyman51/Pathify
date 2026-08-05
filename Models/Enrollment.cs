using System;
using System.Collections.Generic;

namespace Pathify.Models;
public enum PassStatus
{
    Pending = 0,   // لسه مادخلش نتيجة
    Passed = 1,
    Failed = 2
}
public partial class Enrollment
{
    public string CourseId { get; set; } = null!;

    public string StudentSsn { get; set; } = null!;

    public DateOnly? EnrollmentDate { get; set; }

    public string? AdminSsn { get; set; }

    public PassStatus Passed { get; set; }

    public virtual Adminstration? AdminSsnNavigation { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Student StudentSsnNavigation { get; set; } = null!;
}
 