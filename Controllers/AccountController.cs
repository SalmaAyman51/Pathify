using ClosedXML.Excel;
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
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ✅ لازم بيانات الطالب تكون موجودة بالفعل في TempStudentData
            // (الأدمن عملها Import من Excel قبل كده)
            var tempStudent = await _context.TempStudentData
                .FirstOrDefaultAsync(t => t.SSN == dto.SSN);

            if (tempStudent == null)
                return BadRequest("SSN not found. Please contact admin to verify your data was imported.");

            // ✅ لو الطالب ده اتعمله Account قبل كده
            if (!string.IsNullOrEmpty(tempStudent.Email))
            {
                var existingUserBySsn = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.SSN == dto.SSN);

                if (existingUserBySsn != null)
                    return BadRequest("An account already exists for this SSN.");
            }

            // ✅ Check لو الطالب موجود بالفعل (اتعمله Approve) في Students
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == dto.SSN);

            if (existingStudent != null)
                return BadRequest("Student already exists in the system");

            // ✅ إنشاء الـ User Account بس - بياناته الأساسية جايه من Temp اللي الأدمن عمله Import
            var user = new ApplicationUser
            {
                UserName = tempStudent.SSN,   // لو الإيميل اتحط وقت الـ Import
                Email = tempStudent.Email,
                PhoneNumber = tempStudent.PhoneNumber,
                SSN = dto.SSN,
                IsApproved = false               // لسه مستني Approve من الأدمن
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Student");

            // ✅ مش هننشئ TempStudentData تاني، هو موجود بالفعل من الـ Import
            // ممكن نعمله ربط بالـ UserId لو الجدول فيه عمود زي كده
            tempStudent.UserId = user.Id; // لو الجدول فيه FK على User
            await _context.SaveChangesAsync();

            return Ok("Registered successfully, waiting for admin approval");
        }








        [HttpPost("register-professor")]
        public async Task<IActionResult> RegisterProfessor([FromBody] RegisterProfessorDto dto)
        {
            // ✅ تأكدي إن مفيش حساب موجود بالفعل بنفس الـ SSN
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.SSN == dto.SSN);

            if (existingUser != null)
                return BadRequest("An account already exists for this SSN");

            string email;
            string fullName;
            string role;
            string professorType;

            // ✅ بندور بالـ SSN في الجدولين، مش بناخد كلام الدكتور إنه Internal ولا External
            var internalProf = await _context.InternalProfessors
                .FirstOrDefaultAsync(p => p.InternalProfessorSsn == dto.SSN);

            if (internalProf != null)
            {
                email = internalProf.Email;
                fullName = internalProf.InternalProfessorName;
                role = "InternalProfessor";
                professorType = "Internal";
            }
            else
            {
                var externalProf = await _context.ExternalProfessors
                    .FirstOrDefaultAsync(p => p.ExternalProfessorSsn == dto.SSN);

                if (externalProf == null)
                    return BadRequest("SSN not found in Internal or External Professors records. You are not registered as a professor in the system.");

                email = externalProf.Email;
                fullName = externalProf.ExternalProfessorName;
                role = "ExternalProfessor";
                professorType = "External";
            }

            var user = new ApplicationUser
            {
                UserName = dto.SSN,
                Email = email,
                SSN = dto.SSN,
                IsApproved = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            await _userManager.AddToRoleAsync(user, role);

            return Ok(new { message = $"{professorType} professor registered successfully", ssn = dto.SSN, role });
        }
        // ================= LOGIN =================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var user = await _userManager.Users
    .FirstOrDefaultAsync(u => u.SSN == model.SSN);

            if (user == null)
                return Unauthorized("Invalid username or password");

            if (!user.IsApproved)
                return Unauthorized("Your account is waiting for admin approval");

            // اجيبي fresh object بالـ Id
            user = await _userManager.FindByIdAsync(user.Id);

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









        [HttpPost("create-superadmin")]
        public async Task<IActionResult> SeedSuperAdmin()
        {
            var existing = await _userManager.FindByEmailAsync("superadmin@pathify.com");
            if (existing != null)
                return BadRequest("Super admin user already exists");

            var user = new ApplicationUser
            {
                UserName = "superadmin@pathify.com",
                Email = "superadmin@pathify.com",
                SSN = "11111111111111",
                IsApproved = true
            };

            var result = await _userManager.CreateAsync(user, "SuperAdmin@123");
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync("SuperAdmin"))
                await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));

            await _userManager.AddToRoleAsync(user, "SuperAdmin");

            return Ok("Super admin created successfully");
        }
        // ================= APPROVE USER =================

        [Authorize(Roles = "Admin")]
        [HttpPost("approve-user/{ssn}")]
        public async Task<IActionResult> ApproveUser(string ssn)
        {
            var tempStudent = await _context.TempStudentData
                .FirstOrDefaultAsync(t => t.SSN == ssn);

            if (tempStudent == null)
                return NotFound("Student data not found for this SSN");

            if (string.IsNullOrEmpty(tempStudent.UserId))
                return BadRequest("This student hasn't registered an account yet");

            var user = await _userManager.FindByIdAsync(tempStudent.UserId);
            if (user == null) return NotFound("User not found");

            if (user.IsApproved)
                return BadRequest("User is already approved");

            var alreadyExists = await _context.Students
                .AnyAsync(s => s.StudentSsn == tempStudent.SSN);

            if (alreadyExists)
                return BadRequest("Student already exists in the system");

            // 👇 الـ Transaction بتبدأ من هنا، بعد كل الـ validation
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                user.IsApproved = true;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(updateResult.Errors);
                }

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
                    LevelId = tempStudent.LevelId ?? 1,
                    TeamId = tempStudent.TeamId,
                    ProjectId = tempStudent.ProjectId,
                    IsApproved = true,
                    CurrentSemester = tempStudent.CurrentSemester,
                };

                _context.Students.Add(student);
                _context.TempStudentData.Remove(tempStudent);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok("User approved and student created");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Approve failed: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            // ✅ نجيب الـ SSN مباشرة من التوكن بدل الاعتماد على الإيميل
            var ssn = User.FindFirstValue("SSN")
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(ssn))
                return Unauthorized("SSN claim not found in token");

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.SSN == ssn);
            if (user == null)
                return NotFound("User not found");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentSsn == ssn);

            return Ok(new
            {
                user.Email,
                user.SSN,
                user.IsApproved,
                AcademicLevel = student?.AcademicLevel,
                IsSenior = student != null && student.AcademicLevel == "4"
            });
        }
        // ================= FORGET PASSWORD =================

        // 1️⃣ طلب الكود
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            // 1. تنظيف الإيميل وتحويله لحروف صغيرة
            var email = model.Email?.ToLower().Trim();

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email cannot be empty");
            }

            // 2. البحث عن المستخدم في الداتا بيز بالإيميل
            var user = await _userManager.FindByEmailAsync(email);

            // 3. لو ملاقاهوش بالإيميل، نجرب نبحث عنه كـ Username (اختياري حسب نظامكم)
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(email);
            }

            // 4. 🚨 الشرط القاطع: لو بعد البحثين برضه user بـ null.. ارفع كارت أحمر واقفل فوراً!
            if (user == null)
            {
                return NotFound("This email or username is not registered in our system.");
            }

            // ----------------------------------------------------
            // الكود مش هيريد يوصل هنا إلا لو الـ user موجود فعلاً
            // ----------------------------------------------------

            // 5. توليد كود التحقق وحفظه
            var code = new Random().Next(100000, 999999).ToString();
            await _userManager.SetAuthenticationTokenAsync(user, "PasswordReset", "ResetCode", code);

            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Pathify", senderEmail));
                message.To.Add(new MailboxAddress("", user.Email));
                message.Subject = "Password Reset Code";
                message.Body = new TextPart("plain")
                {
                    Text = $"Your password reset code is: {code}"
                };

                using var client = new SmtpClient();

                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, senderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return Ok("Reset code sent to your email");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending email: {ex.Message}");
            }
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

            var savedCode = await _userManager.GetAuthenticationTokenAsync(user, "PasswordReset", "ResetCode");
            if (savedCode != model.Code) return BadRequest("Invalid code");

            // ✅ الحل: استخدمي token رسمي من Identity
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            await _userManager.RemoveAuthenticationTokenAsync(user, "PasswordReset", "ResetCode");

            // ✅ مهم: update الـ SecurityStamp عشان الـ login يشتغل صح
            await _userManager.UpdateSecurityStampAsync(user);

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