using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DsaVisual.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dedupe trước khi tạo unique index — DB cũ có thể đã chứa trùng do seed song song / crash giữa chừng.
            // Giữ bản ghi MỚI NHẤT (MAX Id) mỗi nhóm, xóa các bản trùng (pattern AddSubmissionUniqueConstraints).
            // Lessons/Exercises chỉ dedupe hàng ĐANG HOẠT ĐỘNG (DeletedAt IS NULL) — index filter cũng chỉ
            // ràng buộc các hàng đó; hàng đã xóa mềm không được phép bị hard-delete.
            migrationBuilder.Sql("""
                DELETE t FROM Lessons t
                INNER JOIN (
                    SELECT TopicId, Title, MAX(Id) AS KeepId
                    FROM Lessons
                    WHERE DeletedAt IS NULL
                    GROUP BY TopicId, Title
                    HAVING COUNT(*) > 1
                ) d ON t.TopicId = d.TopicId AND t.Title = d.Title
                   AND t.Id <> d.KeepId
                WHERE t.DeletedAt IS NULL;
                """);
            migrationBuilder.Sql("""
                DELETE t FROM Exercises t
                INNER JOIN (
                    SELECT LessonId, Title, MAX(Id) AS KeepId
                    FROM Exercises
                    WHERE DeletedAt IS NULL
                    GROUP BY LessonId, Title
                    HAVING COUNT(*) > 1
                ) d ON t.LessonId = d.LessonId AND t.Title = d.Title
                   AND t.Id <> d.KeepId
                WHERE t.DeletedAt IS NULL;
                """);
            migrationBuilder.Sql("""
                DELETE t FROM LearningPaths t
                INNER JOIN (
                    SELECT Title, MAX(Id) AS KeepId
                    FROM LearningPaths
                    GROUP BY Title
                    HAVING COUNT(*) > 1
                ) d ON t.Title = d.Title
                   AND t.Id <> d.KeepId;
                """);
            migrationBuilder.Sql("""
                DELETE t FROM LearningPathNodes t
                INNER JOIN (
                    SELECT PathId, Title, MAX(Id) AS KeepId
                    FROM LearningPathNodes
                    GROUP BY PathId, Title
                    HAVING COUNT(*) > 1
                ) d ON t.PathId = d.PathId AND t.Title = d.Title
                   AND t.Id <> d.KeepId;
                """);
            migrationBuilder.Sql("""
                DELETE t FROM LessonSimulations t
                INNER JOIN (
                    SELECT LessonId, SimulationKey, MAX(Id) AS KeepId
                    FROM LessonSimulations
                    GROUP BY LessonId, SimulationKey
                    HAVING COUNT(*) > 1
                ) d ON t.LessonId = d.LessonId AND t.SimulationKey = d.SimulationKey
                   AND t.Id <> d.KeepId;
                """);

            migrationBuilder.DropIndex(
                name: "IX_LessonSimulations_LessonId",
                table: "LessonSimulations");

            migrationBuilder.AlterColumn<string>(
                name: "SimulationKey",
                table: "LessonSimulations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSimulations_LessonId_SimulationKey",
                table: "LessonSimulations",
                columns: new[] { "LessonId", "SimulationKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_TopicId_Title",
                table: "Lessons",
                columns: new[] { "TopicId", "Title" },
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_Title",
                table: "LearningPaths",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathNodes_PathId_Title",
                table: "LearningPathNodes",
                columns: new[] { "PathId", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_LessonId_Title",
                table: "Exercises",
                columns: new[] { "LessonId", "Title" },
                unique: true,
                filter: "[DeletedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LessonSimulations_LessonId_SimulationKey",
                table: "LessonSimulations");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_TopicId_Title",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_LearningPaths_Title",
                table: "LearningPaths");

            migrationBuilder.DropIndex(
                name: "IX_LearningPathNodes_PathId_Title",
                table: "LearningPathNodes");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_LessonId_Title",
                table: "Exercises");

            migrationBuilder.AlterColumn<string>(
                name: "SimulationKey",
                table: "LessonSimulations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_LessonSimulations_LessonId",
                table: "LessonSimulations",
                column: "LessonId");
        }
    }
}
