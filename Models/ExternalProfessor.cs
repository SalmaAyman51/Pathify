using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class ExternalProfessor
{
    public string ExternalProfessorSsn { get; set; } = null!;

    public string ExternalProfessorName { get; set; } = null!;

    public string DeptName { get; set; } = null!;
    public string Email { get; set; } = null!;

    public virtual ICollection<ExternalProfessorPhone> ExternalProfessorPhones { get; set; } = new List<ExternalProfessorPhone>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Supervisor> Supervisors { get; set; } = new List<Supervisor>();
}
