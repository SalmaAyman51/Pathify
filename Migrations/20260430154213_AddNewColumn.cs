using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pathify.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TeamId",
                table: "TempStudentData",
                newName: "team_id");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "TempStudentData",
                newName: "project_id");

            migrationBuilder.AddColumn<string>(
                name: "CourseType",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseType",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "team_id",
                table: "TempStudentData",
                newName: "TeamId");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "TempStudentData",
                newName: "ProjectId");
        }
    }
}
