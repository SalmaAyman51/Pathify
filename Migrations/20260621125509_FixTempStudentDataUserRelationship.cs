using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pathify.Migrations
{
    /// <inheritdoc />
    public partial class FixTempStudentDataUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_TempStudentData_TempStudentDataSSN",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TempStudentDataSSN",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TempStudentDataSSN",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TempStudentDataSSN",
                table: "AspNetUsers",
                type: "nvarchar(14)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TempStudentDataSSN",
                table: "AspNetUsers",
                column: "TempStudentDataSSN");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_TempStudentData_TempStudentDataSSN",
                table: "AspNetUsers",
                column: "TempStudentDataSSN",
                principalTable: "TempStudentData",
                principalColumn: "SSN");
        }
    }
}
