using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pathify.Migrations
{
    /// <inheritdoc />
    public partial class FixProfessorColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_External_Professor_ExternalProfessorSsn",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Internal_Professor_InternalProfessorSsn",
                table: "Teams");

            migrationBuilder.RenameColumn(
                name: "InternalProfessorSsn",
                table: "Teams",
                newName: "internal_professor_SSN");

            migrationBuilder.RenameColumn(
                name: "ExternalProfessorSsn",
                table: "Teams",
                newName: "external_professor_SSN");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_InternalProfessorSsn",
                table: "Teams",
                newName: "IX_Teams_internal_professor_SSN");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_ExternalProfessorSsn",
                table: "Teams",
                newName: "IX_Teams_external_professor_SSN");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_External_Professor_external_professor_SSN",
                table: "Teams",
                column: "external_professor_SSN",
                principalTable: "External_Professor",
                principalColumn: "external_professor_SSN",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Internal_Professor_internal_professor_SSN",
                table: "Teams",
                column: "internal_professor_SSN",
                principalTable: "Internal_Professor",
                principalColumn: "Internal_professor_SSN",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_External_Professor_external_professor_SSN",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Internal_Professor_internal_professor_SSN",
                table: "Teams");

            migrationBuilder.RenameColumn(
                name: "internal_professor_SSN",
                table: "Teams",
                newName: "InternalProfessorSsn");

            migrationBuilder.RenameColumn(
                name: "external_professor_SSN",
                table: "Teams",
                newName: "ExternalProfessorSsn");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_internal_professor_SSN",
                table: "Teams",
                newName: "IX_Teams_InternalProfessorSsn");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_external_professor_SSN",
                table: "Teams",
                newName: "IX_Teams_ExternalProfessorSsn");

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
    }
}
