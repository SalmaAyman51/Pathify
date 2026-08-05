using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pathify.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectedByToProjectProposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "ProjectProposals",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "ProjectProposals");
        }
    }
}
