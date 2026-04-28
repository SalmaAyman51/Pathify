using Microsoft.AspNetCore.Identity;

namespace Pathify.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsApproved { get; set; } = false;
        public string? SSN { get; set; }

    }
}

