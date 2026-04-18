using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class InternalProfessor
{
    public string InternalProfessorSsn { get; set; } = null!;

    public string InternalProfessorName { get; set; } = null!;

    public string DeptName { get; set; } = null!;

    public virtual ICollection<InternalProfessorPhone> InternalProfessorPhones { get; set; } = new List<InternalProfessorPhone>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Supervisor> Supervisors { get; set; } = new List<Supervisor>();
}
