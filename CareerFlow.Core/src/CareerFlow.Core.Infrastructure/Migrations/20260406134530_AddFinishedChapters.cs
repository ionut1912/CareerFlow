using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerFlow.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinishedChapters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "finished_chapters",
                table: "UserProfiles",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "finished_chapters",
                table: "UserProfiles");
        }
    }
}
