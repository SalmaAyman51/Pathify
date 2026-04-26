using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pathify.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adminstration",
                columns: table => new
                {
                    AdminSSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    FName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(101)", maxLength: 101, nullable: false, computedColumnSql: "(([FName]+' ')+[LName])", stored: false),
                    Role = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    managerSSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adminstration", x => x.AdminSSN);
                    table.ForeignKey(
                        name: "FK_Adminstration_Adminstration",
                        column: x => x.managerSSN,
                        principalTable: "Adminstration",
                        principalColumn: "AdminSSN");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SSN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Major = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnrollmentYear = table.Column<int>(type: "int", nullable: false),
                    GPA = table.Column<double>(type: "float", nullable: false),
                    AcademicLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "External_Professor",
                columns: table => new
                {
                    external_professor_SSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    external_professor_name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    dept_name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__External__2CA6D419ACC08BAD", x => x.external_professor_SSN);
                });

            migrationBuilder.CreateTable(
                name: "Internal_Professor",
                columns: table => new
                {
                    Internal_professor_SSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    Internal_professor_name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    dept_name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Internal__251BDFA849DE7D95", x => x.Internal_professor_SSN);
                });

            migrationBuilder.CreateTable(
                name: "Levels",
                columns: table => new
                {
                    LevelID = table.Column<int>(type: "int", nullable: false),
                    LevelName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Levels__09F03C061CA2AE55", x => x.LevelID);
                });

            migrationBuilder.CreateTable(
                name: "Admin_phone",
                columns: table => new
                {
                    AdminSSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Admin_ph__BF0595A377126546", x => new { x.AdminSSN, x.PhoneNumber });
                    table.ForeignKey(
                        name: "FK_Admin_phone_Adminstration",
                        column: x => x.AdminSSN,
                        principalTable: "Adminstration",
                        principalColumn: "AdminSSN");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "External_Professor_phone",
                columns: table => new
                {
                    external_professor_SSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    phone_number = table.Column<string>(type: "varchar(11)", unicode: false, maxLength: 11, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__External__2359072B2B737C0D", x => new { x.phone_number, x.external_professor_SSN });
                    table.ForeignKey(
                        name: "FK_External_Professor_phone_External_Professor",
                        column: x => x.external_professor_SSN,
                        principalTable: "External_Professor",
                        principalColumn: "external_professor_SSN");
                });

            migrationBuilder.CreateTable(
                name: "Internal_Professor_phone",
                columns: table => new
                {
                    Internal_professor_SSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    phone_number = table.Column<string>(type: "varchar(11)", unicode: false, maxLength: 11, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Internal__33C2D790883CEC2A", x => new { x.phone_number, x.Internal_professor_SSN });
                    table.ForeignKey(
                        name: "FK_Internal_Professor_phone_Internal_Professor",
                        column: x => x.Internal_professor_SSN,
                        principalTable: "Internal_Professor",
                        principalColumn: "Internal_professor_SSN");
                });

            migrationBuilder.CreateTable(
                name: "project",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "int", nullable: false),
                    team_id = table.Column<int>(type: "int", nullable: false),
                    external_professor_SSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    internal_professor_SSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    project_name = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Project_description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__project___BC799E1FF0F2C05A", x => x.project_id);
                    table.ForeignKey(
                        name: "FK_project_team_External_Professor",
                        column: x => x.external_professor_SSN,
                        principalTable: "External_Professor",
                        principalColumn: "external_professor_SSN");
                    table.ForeignKey(
                        name: "FK_project_team_Internal_Professor",
                        column: x => x.internal_professor_SSN,
                        principalTable: "Internal_Professor",
                        principalColumn: "Internal_professor_SSN");
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Course_Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Course_Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Course_semester = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    DepartmentName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    AdminSSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    CourseLevel = table.Column<int>(type: "int", nullable: false),
                    PreReqCourseID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreditHours = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Courses__37E005DBC529ABC7", x => x.Course_Id);
                    table.ForeignKey(
                        name: "FK_Courses_Adminstration",
                        column: x => x.AdminSSN,
                        principalTable: "Adminstration",
                        principalColumn: "AdminSSN");
                    table.ForeignKey(
                        name: "FK_Courses_Courses",
                        column: x => x.PreReqCourseID,
                        principalTable: "Courses",
                        principalColumn: "Course_Id");
                    table.ForeignKey(
                        name: "FK_Courses_Levels",
                        column: x => x.CourseLevel,
                        principalTable: "Levels",
                        principalColumn: "LevelID");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentSSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: true),
                    FName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(101)", maxLength: 101, nullable: false, computedColumnSql: "(([FName]+' ')+[LName])", stored: false),
                    GPA = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnrollmentYear = table.Column<int>(type: "int", nullable: true),
                    AcademicLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LevelID = table.Column<int>(type: "int", nullable: true),
                    team_id = table.Column<int>(type: "int", nullable: true),
                    project_id = table.Column<int>(type: "int", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Students__A34E12E9FC908EE4", x => x.StudentSSN);
                    table.ForeignKey(
                        name: "FK_Students_project",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "project_id");
                });

            migrationBuilder.CreateTable(
                name: "supervisors",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "int", nullable: false),
                    internal_professor_SSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    external_professor_SSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    project_name = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__supervis__FFC582F35A74EF39", x => new { x.project_id, x.internal_professor_SSN, x.external_professor_SSN });
                    table.ForeignKey(
                        name: "FK_supervisors_External_Professor",
                        column: x => x.external_professor_SSN,
                        principalTable: "External_Professor",
                        principalColumn: "external_professor_SSN");
                    table.ForeignKey(
                        name: "FK_supervisors_Internal_Professor",
                        column: x => x.internal_professor_SSN,
                        principalTable: "Internal_Professor",
                        principalColumn: "Internal_professor_SSN");
                    table.ForeignKey(
                        name: "FK_supervisors_project_team",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "project_id");
                });

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    Course_Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StudentSSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    Enrollment_Date = table.Column<DateOnly>(type: "date", nullable: true),
                    AdminSSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: true),
                    passed = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Enrollme__BDD4E4F595056AF2", x => new { x.Course_Id, x.StudentSSN });
                    table.ForeignKey(
                        name: "FK_Enrollment_Adminstration",
                        column: x => x.AdminSSN,
                        principalTable: "Adminstration",
                        principalColumn: "AdminSSN");
                    table.ForeignKey(
                        name: "FK_Enrollment_Courses",
                        column: x => x.Course_Id,
                        principalTable: "Courses",
                        principalColumn: "Course_Id");
                    table.ForeignKey(
                        name: "FK_Enrollment_Students",
                        column: x => x.StudentSSN,
                        principalTable: "Students",
                        principalColumn: "StudentSSN");
                });

            migrationBuilder.CreateTable(
                name: "SelectedCourses",
                columns: table => new
                {
                    StudentSsn = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedCourses", x => new { x.StudentSsn, x.CourseId });
                    table.ForeignKey(
                        name: "FK_SelectedCourses_Course",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Course_Id");
                    table.ForeignKey(
                        name: "FK_SelectedCourses_Student",
                        column: x => x.StudentSsn,
                        principalTable: "Students",
                        principalColumn: "StudentSSN");
                });

            migrationBuilder.CreateTable(
                name: "student_phone",
                columns: table => new
                {
                    StudentSSN = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__student___2B11A60A9ECCBC2D", x => new { x.StudentSSN, x.PhoneNumber });
                    table.ForeignKey(
                        name: "FK_student_phone_Students",
                        column: x => x.StudentSSN,
                        principalTable: "Students",
                        principalColumn: "StudentSSN");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adminstration_managerSSN",
                table: "Adminstration",
                column: "managerSSN");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_AdminSSN",
                table: "Courses",
                column: "AdminSSN");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CourseLevel",
                table: "Courses",
                column: "CourseLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_PreReqCourseID",
                table: "Courses",
                column: "PreReqCourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_AdminSSN",
                table: "Enrollment",
                column: "AdminSSN");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_StudentSSN",
                table: "Enrollment",
                column: "StudentSSN");

            migrationBuilder.CreateIndex(
                name: "IX_External_Professor_phone_external_professor_SSN",
                table: "External_Professor_phone",
                column: "external_professor_SSN");

            migrationBuilder.CreateIndex(
                name: "IX_Internal_Professor_phone_Internal_professor_SSN",
                table: "Internal_Professor_phone",
                column: "Internal_professor_SSN");

            migrationBuilder.CreateIndex(
                name: "IX_project_external_professor_SSN",
                table: "project",
                column: "external_professor_SSN");

            migrationBuilder.CreateIndex(
                name: "IX_project_internal_professor_SSN",
                table: "project",
                column: "internal_professor_SSN");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedCourses_CourseId",
                table: "SelectedCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_project_id",
                table: "Students",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Students__32C52A78EC5DD016",
                table: "Students",
                column: "StudentID",
                unique: true,
                filter: "[StudentID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Students__A9D10534243C9339",
                table: "Students",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supervisors_external_professor_SSN",
                table: "supervisors",
                column: "external_professor_SSN");

            migrationBuilder.CreateIndex(
                name: "IX_supervisors_internal_professor_SSN",
                table: "supervisors",
                column: "internal_professor_SSN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin_phone");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "External_Professor_phone");

            migrationBuilder.DropTable(
                name: "Internal_Professor_phone");

            migrationBuilder.DropTable(
                name: "SelectedCourses");

            migrationBuilder.DropTable(
                name: "student_phone");

            migrationBuilder.DropTable(
                name: "supervisors");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Adminstration");

            migrationBuilder.DropTable(
                name: "Levels");

            migrationBuilder.DropTable(
                name: "project");

            migrationBuilder.DropTable(
                name: "External_Professor");

            migrationBuilder.DropTable(
                name: "Internal_Professor");
        }
    }
}
