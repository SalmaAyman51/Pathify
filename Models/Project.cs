using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pathify.Models;

public partial class Project
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProjectId { get; set; }

    public int TeamId { get; set; }

    public string ExternalProfessorSsn { get; set; } = null!;

    public string InternalProfessorSsn { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public string ProjectDescription { get; set; } = null!;

    public virtual ExternalProfessor ExternalProfessorSsnNavigation { get; set; } = null!;

    public virtual InternalProfessor InternalProfessorSsnNavigation { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Supervisor> Supervisors { get; set; } = new List<Supervisor>();
}
