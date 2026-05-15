using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class TeamMember
{
    public int TeamId { get; set; }

    public string StudentSsn { get; set; } = null!;

    public bool IsLeader { get; set; }
    public bool IsApproved { get; set; }

    public virtual Team Team { get; set; } = null!;
    
    public virtual Student Student { get; set; }

}
