using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class StudentPhone
{
    public string StudentSsn { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual Student StudentSsnNavigation { get; set; } = null!;
}
