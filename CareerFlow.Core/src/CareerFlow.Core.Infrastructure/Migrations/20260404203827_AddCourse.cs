using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerFlow.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CoreConcept = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chapters_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chapters_Courses_CourseId1",
                        column: x => x.CourseId1,
                        principalTable: "Courses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "course_user_profiles",
                columns: table => new
                {
                    CoursesId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfilesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_user_profiles", x => new { x.CoursesId, x.UserProfilesId });
                    table.ForeignKey(
                        name: "FK_course_user_profiles_Courses_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_user_profiles_UserProfiles_UserProfilesId",
                        column: x => x.UserProfilesId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubChapters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TheoryHtml = table.Column<string>(type: "text", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubChapters_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_CourseId",
                table: "Chapters",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_CourseId1",
                table: "Chapters",
                column: "CourseId1");

            migrationBuilder.CreateIndex(
                name: "IX_course_user_profiles_UserProfilesId",
                table: "course_user_profiles",
                column: "UserProfilesId");

            migrationBuilder.CreateIndex(
                name: "IX_SubChapters_ChapterId",
                table: "SubChapters",
                column: "ChapterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "course_user_profiles");

            migrationBuilder.DropTable(
                name: "SubChapters");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Courses");
        }
    }
}
