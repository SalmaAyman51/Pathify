using System;
using System.Collections.Generic;

namespace Pathify.Models;

public partial class AdminPhone
{
    public string AdminSsn { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual Adminstration AdminSsnNavigation { get; set; } = null!;
}
