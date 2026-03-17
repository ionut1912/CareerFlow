using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerFlow.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserIdToAcountId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserProfiles",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                newName: "IX_UserProfiles_AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "UserProfiles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserProfiles_AccountId",
                table: "UserProfiles",
                newName: "IX_UserProfiles_UserId");
        }
    }
}
