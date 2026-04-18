using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class InternalProfessorPhone
{
    public string InternalProfessorSsn { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual InternalProfessor InternalProfessorSsnNavigation { get; set; } = null!;
}
