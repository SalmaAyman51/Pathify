namespace Pathify.Controllers
{
    public class ResetPasswordDto
    {
        public string NewPassword { get; internal set; }
        public string Email { get; internal set; }
        public string? Code { get; internal set; }
    }
}