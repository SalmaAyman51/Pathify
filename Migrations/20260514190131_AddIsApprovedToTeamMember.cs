using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pathify.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedToTeamMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "SelectedCourses");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Internal_Professor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "External_Professor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaderSsn = table.Column<string>(type: "varchar(14)", nullable: false),
                    MaxMembers = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_Teams_Students_LeaderSsn",
                        column: x => x.LeaderSsn,
                        principalTable: "Students",
                        principalColumn: "StudentSSN",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    StudentSsn = table.Column<string>(type: "varchar(14)", nullable: false),
                    IsLeader = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => new { x.TeamId, x.StudentSsn });
                    table.ForeignKey(
                        name: "FK_TeamMembers_Students_StudentSsn",
                        column: x => x.StudentSsn,
                        principalTable: "Students",
                        principalColumn: "StudentSSN",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_StudentSsn",
                table: "TeamMembers",
                column: "StudentSsn");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_LeaderSsn",
                table: "Teams",
                column: "LeaderSsn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Internal_Professor");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "External_Professor");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "SelectedCourses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
