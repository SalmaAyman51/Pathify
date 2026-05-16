using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Pathify.Data;
using Pathify.DTOs;
using Pathify.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Pathify.DTOs.RegisterDTO;


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
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ✅ Check لو الطالب موجود بالفعل في Students
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == dto.SSN
                                        || s.StudentId == dto.StudentId
                                        || s.Email == dto.Email);

            if (existingStudent != null)
                return BadRequest("Student already exists in the system");

            // ✅ Check لو موجود في TempStudentData
            var existingTemp = await _context.TempStudentData
                .FirstOrDefaultAsync(t => t.SSN == dto.SSN
                                        || t.StudentId == dto.StudentId
                                        || t.Email == dto.Email);

            if (existingTemp != null)
                return BadRequest("Student already registered and waiting for approval");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                SSN = dto.SSN,
                IsApproved = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, dto.Role);

            var tempStudent = new TempStudentData
            {
                SSN = dto.SSN,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                StudentId = dto.StudentId,
                Email = dto.Email,
                BirthDate = dto.BirthDate.ToDateTime(TimeOnly.MinValue), // DateOnly → DateTime
                Gender = dto.Gender,
                EnrollmentYear = dto.EnrollmentYear ?? 0,
                GPA = (double)(dto.GPA ?? 0),                           // decimal? → double
                AcademicLevel = dto.AcademicLevel,
                LevelId = dto.LevelId,
                ProjectId = dto.ProjectId,
                TeamId = dto.TeamId,
                CurrentSemester=dto.CurrentSemester,
                PhoneNumber=dto.PhoneNumber
            };

            _context.TempStudentData.Add(tempStudent);
            await _context.SaveChangesAsync();

            return Ok("Registered successfully, waiting for admin approval");
        }
        // ================= LOGIN =================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.SSN == model.SSN);

            if (user == null)
                return Unauthorized("Invalid username or password");

            if (!user.IsApproved)
                return Unauthorized("Your account is waiting for admin approval");

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Invalid SSN or password");

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName!),
        new Claim(ClaimTypes.NameIdentifier, user.SSN), // ← أضفنا ده
        new Claim("SSN", user.SSN)
    };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                expires: DateTime.Now.AddHours(24),
                claims: claims,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                    SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                role = roles.FirstOrDefault()
            });
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
                IsApproved = true,
                CurrentSemester= tempStudent.CurrentSemester,
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
        // ================= FORGET PASSWORD =================

        // 1️⃣ طلب الكود
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email.ToLower().Trim());
            if (user == null)
                user = await _userManager.FindByNameAsync(model.Email.ToLower().Trim());
            if (user == null) return NotFound("Email not found");

            var code = new Random().Next(100000, 999999).ToString();
            await _userManager.SetAuthenticationTokenAsync(user, "PasswordReset", "ResetCode", code);

            // بعت الإيميل
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Pathify", _configuration["EmailSettings:SenderEmail"]));
            message.To.Add(new MailboxAddress("", user.Email));
            message.Subject = "Password Reset Code";
            message.Body = new TextPart("plain")
            {
                Text = $"Your password reset code is: {code}"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_configuration["EmailSettings:SmtpHost"], 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_configuration["EmailSettings:SenderEmail"], _configuration["EmailSettings:SenderPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return Ok("Reset code sent to your email");
        }

        // 2️⃣ التحقق من الكود
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound("Email not found");

            var savedCode = await _userManager.GetAuthenticationTokenAsync(user, "PasswordReset", "ResetCode");
            if (savedCode != model.Code) return BadRequest("Invalid code");

            return Ok("Code is valid");
        }

        // 3️⃣ تغيير الباسورد
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound("Email not found");

            // ✅ تأكد إن الكود صح
            var savedCode = await _userManager.GetAuthenticationTokenAsync(user, "PasswordReset", "ResetCode");
            if (savedCode != model.Code) return BadRequest("Invalid code");

            // ✅ غير الباسورد
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // ✅ امسح الكود بعد الاستخدام
            await _userManager.RemoveAuthenticationTokenAsync(user, "PasswordReset", "ResetCode");

            return Ok("Password reset successfully");
        }
        [HttpDelete("admin-delete/{ssn}")]
        public async Task<ActionResult> DeleteProfessor(string ssn)
        {
            // ✅ شوف في الأول لو في supervisors مرتبطة بيه
            var internalProf = await _context.InternalProfessors.FindAsync(ssn);
            if (internalProf != null)
            {
                // ✅ امسح الـ Supervisors المرتبطة بيه الأول
                var supervisors = await _context.Supervisors
                    .Where(s => s.InternalProfessorSsn == ssn)
                    .ToListAsync();
                if (supervisors.Any())
                    _context.Supervisors.RemoveRange(supervisors);

                // ✅ امسح الـ Projects المرتبطة بيه
                var projects = await _context.Projects
                    .Where(p => p.InternalProfessorSsn == ssn)
                    .ToListAsync();
                if (projects.Any())
                    _context.Projects.RemoveRange(projects);

                // ✅ امسح الـ InternalProfessorPhones المرتبطة بيه
                var phones = await _context.InternalProfessorPhones
                    .Where(p => p.InternalProfessorSsn == ssn)
                    .ToListAsync();
                if (phones.Any())
                    _context.InternalProfessorPhones.RemoveRange(phones);

                _context.InternalProfessors.Remove(internalProf);
            }
            else
            {
                var externalProf = await _context.ExternalProfessors.FindAsync(ssn);
                if (externalProf == null) return NotFound("Professor not found");

                // ✅ امسح الـ Supervisors المرتبطة بيه الأول
                var supervisors = await _context.Supervisors
                    .Where(s => s.ExternalProfessorSsn == ssn)
                    .ToListAsync();
                if (supervisors.Any())
                    _context.Supervisors.RemoveRange(supervisors);

                // ✅ امسح الـ Projects المرتبطة بيه
                var projects = await _context.Projects
                    .Where(p => p.ExternalProfessorSsn == ssn)
                    .ToListAsync();
                if (projects.Any())
                    _context.Projects.RemoveRange(projects);

                // ✅ امسح الـ ExternalProfessorPhones المرتبطة بيه
                var phones = await _context.ExternalProfessorPhones
                    .Where(p => p.ExternalProfessorSsn == ssn)
                    .ToListAsync();
                if (phones.Any())
                    _context.ExternalProfessorPhones.RemoveRange(phones);

                _context.ExternalProfessors.Remove(externalProf);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Deleted Successfully" });
        }
    }
}