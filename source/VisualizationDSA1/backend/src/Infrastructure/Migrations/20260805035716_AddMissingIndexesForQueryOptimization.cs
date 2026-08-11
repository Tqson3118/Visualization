using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingIndexesForQueryOptimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // O3: Add missing indexes for query optimization
            // Index on Classroom.OwnerTeacherId for GetTeacherClassroomsQuery
            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_OwnerTeacherId",
                table: "Classrooms",
                column: "OwnerTeacherId");

            // Index on Course.TeacherId for teacher's course queries
            migrationBuilder.CreateIndex(
                name: "IX_Courses_TeacherId",
                table: "Courses",
                column: "TeacherId");

            // Index on ClassroomEnrollment.ClassroomId for GetClassroomStudentsQuery
            migrationBuilder.CreateIndex(
                name: "IX_ClassroomEnrollments_ClassroomId",
                table: "ClassroomEnrollments",
                column: "ClassroomId");

            // Index on ClassroomEnrollment.StudentId for student classroom queries
            migrationBuilder.CreateIndex(
                name: "IX_ClassroomEnrollments_StudentId",
                table: "ClassroomEnrollments",
                column: "StudentId");

            // Composite index on ClassroomEnrollment (ClassroomId, Status) for filtering active enrollments
            migrationBuilder.CreateIndex(
                name: "IX_ClassroomEnrollments_ClassroomId_Status",
                table: "ClassroomEnrollments",
                columns: new[] { "ClassroomId", "Status" });

            // Index on UserModuleItemProgress.UserId for progress queries
            migrationBuilder.CreateIndex(
                name: "IX_UserModuleItemProgresses_UserId",
                table: "UserModuleItemProgresses",
                column: "UserId");

            // Index on Quiz.Topic for analytics queries
            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_Topic",
                table: "Quizzes",
                column: "Topic");

            // Index on Quiz.Difficulty for analytics queries
            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_Difficulty",
                table: "Quizzes",
                column: "Difficulty");

            // Index on LessonComment.LessonId for lesson comments
            migrationBuilder.CreateIndex(
                name: "IX_LessonComments_LessonId",
                table: "LessonComments",
                column: "LessonId");

            // Existing migration operations
            migrationBuilder.DropTable(
                name: "CheatSheetSnippets");

            migrationBuilder.DropTable(
                name: "UserRoadmapLanguages");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_QuizId",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_UserId",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Hints",
                table: "Codelabs");

            migrationBuilder.CreateTable(
                name: "CodelabHints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodelabId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsTiered = table.Column<bool>(type: "boolean", nullable: false),
                    XpCost = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodelabHints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodelabHints_Codelabs_CodelabId",
                        column: x => x.CodelabId,
                        principalTable: "Codelabs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizId_Score",
                table: "QuizAttempts",
                columns: new[] { "QuizId", "Score" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_UserId_AttemptedAt",
                table: "QuizAttempts",
                columns: new[] { "UserId", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CodelabHints_CodelabId",
                table: "CodelabHints",
                column: "CodelabId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the new indexes in reverse order
            migrationBuilder.DropIndex(
                name: "IX_LessonComments_LessonId",
                table: "LessonComments");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_Difficulty",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_Topic",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_UserModuleItemProgresses_UserId",
                table: "UserModuleItemProgresses");

            migrationBuilder.DropIndex(
                name: "IX_ClassroomEnrollments_ClassroomId_Status",
                table: "ClassroomEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_ClassroomEnrollments_StudentId",
                table: "ClassroomEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_ClassroomEnrollments_ClassroomId",
                table: "ClassroomEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_Courses_TeacherId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Classrooms_OwnerTeacherId",
                table: "Classrooms");

            migrationBuilder.DropTable(
                name: "CodelabHints");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_QuizId_Score",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_UserId_AttemptedAt",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications");

            migrationBuilder.AddColumn<string>(
                name: "Hints",
                table: "Codelabs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CheatSheetSnippets",
                columns: table => new
                {
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataStructure = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CodeSnippet = table.Column<string>(type: "text", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true),
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheatSheetSnippets", x => new { x.Language, x.DataStructure });
                });

            migrationBuilder.CreateTable(
                name: "UserRoadmapLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RoadmapId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoadmapLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoadmapLanguages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizId",
                table: "QuizAttempts",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_UserId",
                table: "QuizAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoadmapLanguages_UserId_RoadmapId",
                table: "UserRoadmapLanguages",
                columns: new[] { "UserId", "RoadmapId" },
                unique: true);
        }
    }
}
