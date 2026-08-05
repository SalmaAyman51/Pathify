using System.ComponentModel.DataAnnotations;

namespace Pathify.DTOs
{
    public class RegisterDTO
    {

       
   
            [Required]
            public string SSN { get; set; }

            [Required]
            public string Password { get; set; }

            [Required]
            [Compare("Password", ErrorMessage = "Password and Confirm Password do not match")]
            public string ConfirmPassword { get; set; }
        
    }
}
