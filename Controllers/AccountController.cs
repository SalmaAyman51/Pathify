using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            // ✅ Check لو الطالب موجود بالفعل في Students
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == model.SSN
                                        || s.StudentId == model.StudentId
                                        || s.Email == model.Email);

            if (existingStudent != null)
                return BadRequest("Student already exists in the system");

            // ✅ Check لو موجود في TempStudentData
            var existingTemp = await _context.TempStudentData
                .FirstOrDefaultAsync(t => t.SSN == model.SSN
                                        || t.StudentId == model.StudentId
                                        || t.Email == model.Email);

            if (existingTemp != null)
                return BadRequest("Student already registered and waiting for approval");

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                SSN = model.SSN,
                IsApproved = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, model.Role);

            var tempStudent = new TempStudentData
            {
                SSN = model.SSN,
                FirstName = model.FirstName,
                LastName = model.LastName,
                StudentId = model.StudentId,
                Email = model.Email,
                BirthDate = model.BirthDate,
                Gender = model.Gender,
                EnrollmentYear = model.EnrollmentYear,
                GPA = model.GPA,
                AcademicLevel = model.AcademicLevel,
                LevelId = model.LevelId,
                ProjectId = model.ProjectId,
                TeamId = model.TeamId
            };

            _context.TempStudentData.Add(tempStudent);
            await _context.SaveChangesAsync();

            return Ok("Registered, waiting for admin approval");
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

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await _roleManager.RoleExistsAsync("Student"))
                await _roleManager.CreateAsync(new IdentityRole("Student"));

            var admin = new ApplicationUser
            {
                UserName = "admin@shahd.com",
                Email = "admin@shahd.com",
                //FirstName = "Admin",
                //LastName = "User",
                SSN = "00000000000000",
                //EnrollmentYear = 0,
                //GPA = 0,
                //AcademicLevel = "N/A",
                //BirthDate = DateTime.Now,
                IsApproved = true
            };

            var result = await _userManager.CreateAsync(admin, "Admin1234@");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
                return Ok("Admin created successfully");
            }

            return BadRequest(result.Errors);
        }

        // ================= APPROVE USER =================
       
        [Authorize(Roles = "Admin")]
        [HttpPost("approve-user/{email}")]
        public async Task<IActionResult> ApproveUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found");


            // ✅ جيبي بيانات الطالب من TempStudentData
            var tempStudent = await _context.TempStudentData
                .FirstOrDefaultAsync(t => t.SSN == user.SSN);
            if (tempStudent == null) return NotFound("Student data not found");

            user.IsApproved = true;
            await _userManager.UpdateAsync(user);

            // ✅ انقلي البيانات لجدول Students
            var student = new Student
            {
                StudentSsn = tempStudent.SSN,
                StudentId = tempStudent.StudentId,
                Fname = tempStudent.FirstName,
                Lname = tempStudent.LastName,
                Email = tempStudent.Email,
                BirthDate = DateOnly.FromDateTime(tempStudent.BirthDate),
                Gender = tempStudent.Gender ?? "N/A",
                EnrollmentYear = tempStudent.EnrollmentYear,
                Gpa = (decimal?)tempStudent.GPA,
                AcademicLevel = tempStudent.AcademicLevel,
                LevelId = tempStudent.LevelId ?? 1, // ✅ أضف السطر ده
                TeamId = tempStudent.TeamId,        // ✅ أضف السطر ده
                ProjectId = tempStudent.ProjectId,  // ✅ أضف السطر ده
                IsApproved = true
            };
            _context.Students.Add(student);

            // ✅ امسحي البيانات المؤقتة
            _context.TempStudentData.Remove(tempStudent);

            await _context.SaveChangesAsync();

            return Ok("User approved and student created");
        }
        [Authorize]
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var email = User.Identity.Name;
            var user = await _userManager.FindByEmailAsync(email);

            return Ok(new
            {
                //user.FirstName,
                //user.LastName,
                user.Email,
                user.SSN,
                //user.Gender,
                //user.AcademicLevel,
                //user.EnrollmentYear,
                //user.GPA,
                //user.BirthDate,
                user.IsApproved
            });
        }
    }
}