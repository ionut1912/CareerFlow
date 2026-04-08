using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerFlow.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImplementCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "CourseUploads",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileType",
                table: "CourseUploads");
        }
    }
}
