using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pathify.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessorRejectionReasonsToProjectProposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalRejectionReason",
                table: "ProjectProposals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalRejectionReason",
                table: "ProjectProposals",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalRejectionReason",
                table: "ProjectProposals");

            migrationBuilder.DropColumn(
                name: "InternalRejectionReason",
                table: "ProjectProposals");
        }
    }
}
