using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pathify.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStudentPhoneTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_phone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
