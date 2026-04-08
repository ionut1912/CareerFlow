using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerFlow.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCorrectAnswerLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CorrectAnswer",
                table: "QuizQuestions",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CorrectAnswer",
                table: "QuizQuestions",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10000)",
                oldMaxLength: 10000);
        }
    }
}
