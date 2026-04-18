using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class Level
{
    public int LevelId { get; set; }

    public string? LevelName { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
