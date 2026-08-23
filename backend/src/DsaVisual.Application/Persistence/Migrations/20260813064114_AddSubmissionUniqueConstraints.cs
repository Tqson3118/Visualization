using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DsaVisual.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dedupe trước khi tạo unique index — DB cũ có thể đã chứa nộp trùng (double-submit trước fix).
            // Giữ bản ghi MỚI NHẤT (MAX Id) mỗi nhóm, xóa các bản trùng.
            migrationBuilder.Sql("""
                DELETE t FROM ExerciseSubmissions t
                INNER JOIN (
                    SELECT UserId, ExerciseId, ClassAssignmentId, MAX(Id) AS KeepId
                    FROM ExerciseSubmissions
                    WHERE ClassAssignmentId IS NOT NULL
                    GROUP BY UserId, ExerciseId, ClassAssignmentId
                    HAVING COUNT(*) > 1
                ) d ON t.UserId = d.UserId AND t.ExerciseId = d.ExerciseId
                   AND t.ClassAssignmentId = d.ClassAssignmentId
                   AND t.Id <> d.KeepId;
                """);
            migrationBuilder.Sql("""
                DELETE t FROM ExerciseSubmissions t
                INNER JOIN (
                    SELECT UserId, ExerciseId, MAX(Id) AS KeepId
                    FROM ExerciseSubmissions
                    WHERE ClassAssignmentId IS NULL
                    GROUP BY UserId, ExerciseId
                    HAVING COUNT(*) > 1
                ) d ON t.UserId = d.UserId AND t.ExerciseId = d.ExerciseId
                   AND t.Id <> d.KeepId;
                """);
            migrationBuilder.Sql("""
                DELETE t FROM CodeSubmissions t
                INNER JOIN (
                    SELECT UserId, ExerciseId, MAX(Id) AS KeepId
                    FROM CodeSubmissions
                    GROUP BY UserId, ExerciseId
                    HAVING COUNT(*) > 1
                ) d ON t.UserId = d.UserId AND t.ExerciseId = d.ExerciseId
                   AND t.Id <> d.KeepId;
                """);

            migrationBuilder.AddColumn<bool>(
                name: "IsClientDeclared",
                table: "CodeSubmissions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSubmissions_User_Exercise_Assignment",
                table: "ExerciseSubmissions",
                columns: new[] { "UserId", "ExerciseId", "ClassAssignmentId" },
                unique: true,
                filter: "[ClassAssignmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSubmissions_User_Exercise_NoAssignment",
                table: "ExerciseSubmissions",
                columns: new[] { "UserId", "ExerciseId" },
                unique: true,
                filter: "[ClassAssignmentId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSubmissions_UserId_ExerciseId",
                table: "CodeSubmissions",
                columns: new[] { "UserId", "ExerciseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExerciseSubmissions_User_Exercise_Assignment",
                table: "ExerciseSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseSubmissions_User_Exercise_NoAssignment",
                table: "ExerciseSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_CodeSubmissions_UserId_ExerciseId",
                table: "CodeSubmissions");

            migrationBuilder.DropColumn(
                name: "IsClientDeclared",
                table: "CodeSubmissions");
        }
    }
}
