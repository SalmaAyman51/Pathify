using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pathify.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessorsToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Teams");

            migrationBuilder.AddColumn<string>(
                name: "ExternalProfessorSsn",
                table: "Teams",
                type: "varchar(14)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalProfessorSsn",
                table: "Teams",
                type: "varchar(14)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ExternalProfessorSsn",
                table: "Teams",
                column: "ExternalProfessorSsn");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_InternalProfessorSsn",
                table: "Teams",
                column: "InternalProfessorSsn");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_External_Professor_ExternalProfessorSsn",
                table: "Teams",
                column: "ExternalProfessorSsn",
                principalTable: "External_Professor",
                principalColumn: "external_professor_SSN",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Internal_Professor_InternalProfessorSsn",
                table: "Teams",
                column: "InternalProfessorSsn",
                principalTable: "Internal_Professor",
                principalColumn: "Internal_professor_SSN",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_External_Professor_ExternalProfessorSsn",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Internal_Professor_InternalProfessorSsn",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_ExternalProfessorSsn",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_InternalProfessorSsn",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ExternalProfessorSsn",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "InternalProfessorSsn",
                table: "Teams");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Teams",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
