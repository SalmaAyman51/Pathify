using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pathify.Models;

public partial class ExternalProfessorPhone
{
    public string ExternalProfessorSsn { get; set; } = null!;
    [Required]
    public string PhoneNumber { get; set; } = null!;

    public virtual ExternalProfessor ExternalProfessorSsnNavigation { get; set; } = null!;
}
