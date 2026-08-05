using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class Team
{
    public int TeamId { get; set; }

    public string LeaderSsn { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }

   // public bool IsApproved { get; set; }
    public string? InternalProfessorSsn { get; set; }
    public string? ExternalProfessorSsn { get; set; }

    public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    public virtual Student Leader { get; set; }
    public virtual InternalProfessor? InternalProfessorSsnNavigation { get; set; }
    public virtual ExternalProfessor? ExternalProfessorSsnNavigation { get; set; }

}

