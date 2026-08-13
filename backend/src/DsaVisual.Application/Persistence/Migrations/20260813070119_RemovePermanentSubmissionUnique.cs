using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DsaVisual.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePermanentSubmissionUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "ClientRequestId",
                table: "ExerciseSubmissions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientRequestId",
                table: "CodeSubmissions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSubmissions_User_Exercise_Assignment_ClientRequestId",
                table: "ExerciseSubmissions",
                columns: new[] { "UserId", "ExerciseId", "ClassAssignmentId", "ClientRequestId" },
                unique: true,
                filter: "[ClientRequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CodeSubmissions_User_Exercise_ClientRequestId",
                table: "CodeSubmissions",
                columns: new[] { "UserId", "ExerciseId", "ClientRequestId" },
                unique: true,
                filter: "[ClientRequestId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExerciseSubmissions_User_Exercise_Assignment_ClientRequestId",
                table: "ExerciseSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_CodeSubmissions_User_Exercise_ClientRequestId",
                table: "CodeSubmissions");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "ExerciseSubmissions");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "CodeSubmissions");

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
    }
}
