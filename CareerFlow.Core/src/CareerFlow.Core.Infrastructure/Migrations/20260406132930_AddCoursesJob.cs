using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerFlow.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoursesJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseUploads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseUploads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ChapterId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubChapterId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuizQuestions_SubChapters_SubChapterId",
                        column: x => x.SubChapterId,
                        principalTable: "SubChapters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CourseJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: true),
                    learning_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseJobs_CourseUploads_UploadId",
                        column: x => x.UploadId,
                        principalTable: "CourseUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseJobs_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseJobs_CourseId",
                table: "CourseJobs",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseJobs_UploadId",
                table: "CourseJobs",
                column: "UploadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_course_uploads_user_id",
                table: "CourseUploads",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_questions_chapter_id",
                table: "QuizQuestions",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_questions_sub_chapter_id",
                table: "QuizQuestions",
                column: "SubChapterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseJobs");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.DropTable(
                name: "CourseUploads");
        }
    }
}
