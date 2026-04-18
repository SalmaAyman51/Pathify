using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class Supervisor
{
    public int ProjectId { get; set; }

    public string InternalProfessorSsn { get; set; } = null!;

    public string ExternalProfessorSsn { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public virtual ExternalProfessor ExternalProfessorSsnNavigation { get; set; } = null!;

    public virtual InternalProfessor InternalProfessorSsnNavigation { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
