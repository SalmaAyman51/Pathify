using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Pathify.Data;
using Pathify.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Pathify.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly PathifyContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            PathifyContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

    


        // ================= REGISTER =================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email, // 👈 login بالإيميل
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,

                FirstName = model.FirstName,
                LastName = model.LastName,

                SSN = model.SSN,
                Major = model.Major,
                EnrollmentYear = model.EnrollmentYear,
                GPA = model.GPA,
                AcademicLevel = model.AcademicLevel,
                BirthDate = model.BirthDate,

                IsApproved = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return Ok("Registered, waiting for admin approval");
            }

            return BadRequest(result.Errors);
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            // 👇 login بالإيميل
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");

            if (!user.IsApproved)
                return Unauthorized("Your account is waiting for admin approval");

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var roles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email!),
                     new Claim("SSN", user.SSN)
                };

                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    expires: DateTime.Now.AddHours(1),
                    claims: claims,
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                        SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }

            return Unauthorized("Invalid email or password");
        }

        // ================= ADD ROLE =================
        [HttpPost("add-role")]
        public async Task<IActionResult> AddRole([FromBody] string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(role));

                if (result.Succeeded)
                    return Ok("Role added successfully");

                return BadRequest(result.Errors);
            }

            return BadRequest("Role already exists");
        }

        // ================= ASSIGN ROLE =================
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] UserRole model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("User not found");

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            if (result.Succeeded)
                return Ok("Role assigned successfully");

            return BadRequest(result.Errors);
        }

        // ================= APPROVE USER =================
        [Authorize(Roles = "Admin")]
        [HttpPost("approve-user/{email}")]
        public async Task<IActionResult> ApproveUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return NotFound("User not found");

            user.IsApproved = true;
            await _userManager.UpdateAsync(user);

            // 👇 لو Student → يتحول لجدول Students
            if (await _userManager.IsInRoleAsync(user, "Student"))
            {
                var student = new Student
                {
                    // ❌ بلاش دي عشان type mismatch
                    // StudentId = user.Id,

                    StudentSsn = user.SSN,

                    Fname = user.FirstName,
                    Lname = user.LastName,
                    FullName = user.FirstName + " " + user.LastName,

                    Email = user.Email,

                    // تحويل DateTime → DateOnly
                    BirthDate = DateOnly.FromDateTime(user.BirthDate),

                    // حطي قيمة افتراضية لو مش عندك
                    Gender = "NotSpecified",

                    EnrollmentYear = user.EnrollmentYear,
                    Gpa = (decimal?)user.GPA,
                    AcademicLevel = user.AcademicLevel,

                    IsApproved = true
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }
            return Ok("User approved and student created");
        }
    }
}