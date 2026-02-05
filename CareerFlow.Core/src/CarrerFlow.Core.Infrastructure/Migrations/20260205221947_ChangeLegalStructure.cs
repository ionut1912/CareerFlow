using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarrerFlow.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLegalStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "TermsAndConditions");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "PrivacyPolicies");

            migrationBuilder.AddColumn<bool>(
                name: "PrivacyPolicyAccepted",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TermsAccepted",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivacyPolicyAccepted",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TermsAccepted",
                table: "Accounts");

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "TermsAndConditions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "PrivacyPolicies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
