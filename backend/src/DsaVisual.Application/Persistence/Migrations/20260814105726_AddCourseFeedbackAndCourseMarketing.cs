using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DsaVisual.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseFeedbackAndCourseMarketing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "LearningPaths",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HighlightsJson",
                table: "LearningPaths",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestimonialsJson",
                table: "LearningPaths",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseFeedback",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReplyText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RepliedById = table.Column<int>(type: "int", nullable: true),
                    RepliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseFeedback_LearningPaths_CourseId",
                        column: x => x.CourseId,
                        principalTable: "LearningPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseFeedback_Users_RepliedById",
                        column: x => x.RepliedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseFeedback_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_AuthorId",
                table: "LearningPaths",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseFeedback_CourseId_CreatedAt",
                table: "CourseFeedback",
                columns: new[] { "CourseId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseFeedback_RepliedById",
                table: "CourseFeedback",
                column: "RepliedById");

            migrationBuilder.CreateIndex(
                name: "IX_CourseFeedback_Status_CreatedAt",
                table: "CourseFeedback",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseFeedback_UserId",
                table: "CourseFeedback",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPaths_Users_AuthorId",
                table: "LearningPaths",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningPaths_Users_AuthorId",
                table: "LearningPaths");

            migrationBuilder.DropTable(
                name: "CourseFeedback");

            migrationBuilder.DropIndex(
                name: "IX_LearningPaths_AuthorId",
                table: "LearningPaths");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "LearningPaths");

            migrationBuilder.DropColumn(
                name: "HighlightsJson",
                table: "LearningPaths");

            migrationBuilder.DropColumn(
                name: "TestimonialsJson",
                table: "LearningPaths");
        }
    }
}
