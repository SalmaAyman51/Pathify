using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class ExternalProfessorPhone
{
    public string ExternalProfessorSsn { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual ExternalProfessor ExternalProfessorSsnNavigation { get; set; } = null!;
}
